namespace ChatSupportAPI.Models;

public sealed record Chat
{
    public Chat(string name)
    {
        Name = name;
        SessionId = Guid.NewGuid().ToString("n");
        Lifetime = DateTimeOffset.Now;
        Conversation = new List<string>(new[] { $"System: Hi {name}! How are you today?" });
    }

    public string SessionId { get; private set; }
    public string Name { get; set; }
    public DateTimeOffset Lifetime { get; set; }
    public List<string> Conversation { get; set; }
    public int Retry { get; set; }
    public string AssignedAgent { get; set; }
}
