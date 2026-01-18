namespace TimetableDesigner.Backend.Events.Providers.RabbitMQ;

public class RabbitMQEventQueueBuilder : EventQueueBuilder<RabbitMQEventQueue>
{
    public string Hostname { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ExchangeName { get; set; } = null!;
    public string QueuePrefix { get; set; } = null!;

    public override IDictionary<string, string> GetConnectionParameters() => new Dictionary<string, string>()
    {
        { nameof(Hostname), Hostname },
        { nameof(Port), Port.ToString() },
        { nameof(Username), Username },
        { nameof(Password), Password },
        { nameof(ExchangeName), ExchangeName },
        { nameof(QueuePrefix), QueuePrefix }
    };
}