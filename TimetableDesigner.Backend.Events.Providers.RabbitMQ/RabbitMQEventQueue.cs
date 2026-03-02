using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

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
        if (!connectionParameters.TryGetValue("Retries", out string retriesStr))
        {
            retriesStr = "0";
        }
        int retries = int.Parse(retriesStr);
        if (!connectionParameters.TryGetValue("RetryCooldown", out string retryCooldownStr))
        {
            retryCooldownStr = "1000";
        }
        int retryCooldown = int.Parse(retryCooldownStr);
            
        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = hostname,
            Port = int.Parse(port),
            UserName = username,
            Password = password,
        };
        
        IConnection? connection = null;
        int retryCount = 0;
        Exception lastException = new Exception("Cannot connect to RabbitMQ");
        while (connection is null && (retries < 0 || retryCount < retries))
        {
            try
            {
                using (Task<IConnection> createConnectionTask = factory.CreateConnectionAsync())
                {
                    createConnectionTask.Wait();
                    connection = createConnectionTask.Result;
                }
            }
            catch (Exception ex)
            {
                Thread.Sleep(retryCooldown);
                retryCount++;
                lastException = ex;
            }
        }

        if (connection is null)
        {
            throw lastException;
        }
        
        services.AddSingleton(connection);
        services.AddSingleton<IEventQueuePublisher, RabbitMQEventQueuePublisher>(sp => new RabbitMQEventQueuePublisher(sp.GetRequiredService<IConnection>(), exchangeName));
        services.AddSingleton<IEventQueueSubscriber, RabbitMQEventQueueSubscriber>(sp => new RabbitMQEventQueueSubscriber(sp.GetRequiredService<IConnection>(), exchangeName, queuePrefix));
    }
}