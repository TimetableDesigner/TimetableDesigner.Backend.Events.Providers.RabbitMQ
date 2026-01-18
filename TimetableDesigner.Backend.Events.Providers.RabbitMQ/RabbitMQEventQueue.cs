using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace TimetableDesigner.Backend.Events.Providers.RabbitMQ;

public class RabbitMQEventQueue : EventQueue
{
    public string Hostname { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ExchangeName { get; set; } = null!;
    public string QueuePrefix { get; set; } = null!;

    public override void Setup(IServiceCollection services)
    {
        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = Hostname,
            Port = Port,
            UserName = Username,
            Password = Password,
        };

        Task<IConnection> createConnectionTask = factory.CreateConnectionAsync();
        createConnectionTask.Wait();
        services.AddSingleton(createConnectionTask.Result);
        services.AddSingleton<IEventQueuePublisher, RabbitMQEventQueuePublisher>(sp => new RabbitMQEventQueuePublisher(sp.GetRequiredService<IConnection>(), ExchangeName));
        services.AddSingleton<IEventQueueSubscriber, RabbitMQEventQueueSubscriber>(sp => new RabbitMQEventQueueSubscriber(sp.GetRequiredService<IConnection>(), ExchangeName, QueuePrefix));
    }
}