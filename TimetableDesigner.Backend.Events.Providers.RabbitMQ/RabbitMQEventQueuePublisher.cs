using System.Text.Json;
using RabbitMQ.Client;

namespace TimetableDesigner.Backend.Events.Providers.RabbitMQ;

public class RabbitMQEventQueuePublisher : IEventQueuePublisher
{
    private readonly IConnection _connection;
    private readonly string _exchangeName;
    
    public RabbitMQEventQueuePublisher(IConnection connection, string exchangeName)
    {
        _connection = connection;
        _exchangeName = exchangeName;
    }

    public async Task PublishAsync(string data, Type dataType)
    {
        BasicProperties properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Type = dataType.FullName,
        };
        ReadOnlyMemory<byte> body = JsonSerializer.SerializeToUtf8Bytes(data);
        await using (IChannel channel = await _connection.CreateChannelAsync())
        {
            await channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Direct);
            await channel.BasicPublishAsync(_exchangeName, dataType.FullName!, true, properties, body);
        }
    }
}