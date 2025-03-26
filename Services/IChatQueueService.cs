using ChatSupportAPI.Models;

namespace ChatSupportAPI.Services;

public interface IChatQueueService
{
    void SetAgentsCapacity(int capacity);
    ChatSession? AddSession(string name);
    ChatSession? SendChat(string sesionId, string message);
    ChatSession? EndSession(string sesionId);
    List<string> GetInfo();
    bool IsMaxCapacity();
    void CheckSessionRetryPolicyForTimeout();
    List<ChatSession> ReturnExpiredSessions();
    List<ChatSession> ReturnStartedQueueMessages();
}
