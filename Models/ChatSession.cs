namespace ChatSupportAPI.Models;

public sealed record ChatSession
{
    public ChatSession(string name)
    {
        Name = name;
        SessionId = Guid.NewGuid().ToString("n");
        Conversation = new List<string>(new[] { $"System: Hi {name}! Welcome to our chat support." });
    }

    public string SessionId { get; private set; }
    public string Name { get; set; }
    public DateTime Lifetime { get; set; }
    public List<string> Conversation { get; set; }
    public int Retry { get; set; }
    public string AssignedAgent { get; set; }

    public void AddMessage(string message)
    {
        Conversation.Add(message);
    }

    public bool IsExpired()
    {
        return DateTime.Now >= Lifetime;
    }
}
