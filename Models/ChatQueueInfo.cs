namespace ChatSupportAPI.Models;

public sealed record ChatQueueInfo
{
    public string WorkShift { get; set; }
    public int TeamCapacity { get; set; }
    public int QueueCapacity { get; set; }
    public int TotalAgents { get; set; }
    public int TotalChatCount { get; set; }
    public int OnWaitingQueue { get; set; }
    public List<Chat> Chats { get; set; }
    public List<Agent> Agents { get; set; }
}