using ChatSupportAPI.Models;

namespace ChatSupportAPI.Services;

public interface IChatSupportService : IChatSupportUtilityService
{
    string NewChatSession(string name);
    ChatSession? SendChatMessage(string sessionId, string message);
    bool EndChatSession(string sessionId);
    List<string> GetInfo();
}

public interface IChatSupportUtilityService
{
    void Utility_PingLiveSessions();
    void Utility_RemoveExpiredSessionOnQueue();
    void Utility_AssignWaitingSessionOnQueue();
    void Utility_ChangeTeamBasedOnWorkshift();
}
