using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace TimetableDesigner.Backend.Events.Providers.RabbitMQ;

public class RabbitMQEventQueue : EventQueue<RabbitMQEventQueue>
{

    protected override void Setup(IServiceCollection services, IDictionary<string, string> connectionParameters)
    {
        if (!connectionParameters.TryGetValue("Hostname", out string hostname))
        {
            hostname = "localhost";
        }
        if (!connectionParameters.TryGetValue("Port", out string port))
        {
            port = "5672";
        }
        string username = connectionParameters["Username"];
        string password = connectionParameters["Password"];
        string exchangeName = connectionParameters["ExchangeName"];
        string queuePrefix = connectionParameters["QueuePrefix"];
            
        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = port,
            Port = int.Parse(port),
            UserName = username,
            Password = password,
        };
        
        Task<IConnection> createConnectionTask = factory.CreateConnectionAsync();
        createConnectionTask.Wait();
        services.AddSingleton(createConnectionTask.Result);
        services.AddSingleton<IEventQueuePublisher, RabbitMQEventQueuePublisher>(sp => new RabbitMQEventQueuePublisher(sp.GetRequiredService<IConnection>(), exchangeName));
        services.AddSingleton<IEventQueueSubscriber, RabbitMQEventQueueSubscriber>(sp => new RabbitMQEventQueueSubscriber(sp.GetRequiredService<IConnection>(), exchangeName, queuePrefix));
    }
}