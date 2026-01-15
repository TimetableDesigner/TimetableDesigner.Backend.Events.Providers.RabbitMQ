using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TimetableDesigner.Backend.Events.Providers.RabbitMQ;

public class RabbitMQEventQueueSubscriber : IEventQueueSubscriber
{
    private readonly IConnection _connection;
    private readonly string _exchangeName;
    private readonly string _queuePrefix;
    
    public RabbitMQEventQueueSubscriber(IConnection connection, string exchangeName, string queuePrefix)
    {
        _connection = connection;
        _exchangeName = exchangeName;
        _queuePrefix = queuePrefix;
    }
    
    public void Subscribe<T>(Func<T, Task> handler) where T : class
    {
        Task<IChannel> createChannelTask = _connection.CreateChannelAsync();
        createChannelTask.Wait();
        IChannel channel = createChannelTask.Result;
        
        string queueName = $"{_queuePrefix}-{typeof(T).FullName!}";
        Task exchangeDeclareTask = channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct);
        Task queueDeclareTask = channel.QueueDeclareAsync(queueName, true, false, false);
        Task.WaitAll(exchangeDeclareTask, queueDeclareTask);
        channel.QueueBindAsync(queueName, _exchangeName, typeof(T).FullName!).Wait();
        
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                T? eventData = JsonSerializer.Deserialize<T>(args.Body.Span);
                if (eventData is null)
                {
                    throw new InvalidOperationException("Received null event");
                }
            
                await handler(eventData);
            
                await channel.BasicAckAsync(args.DeliveryTag, false);
            }
            catch
            {
                await channel.BasicNackAsync(args.DeliveryTag, false, false);
            }
        };
        
        channel.BasicConsumeAsync(queueName, false, consumer).Wait();
    }
}