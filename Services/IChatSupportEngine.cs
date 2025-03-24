using ChatSupportAPI.Models;

namespace ChatSupportAPI.Services;

public interface IChatSupportEngine
{
    string StartChat(string name);
    ChatSession? GetChat(string sessionId);
    ChatSession? SendChat(string sessionId, string message);
    ChatSession? DisconnectChat(string sessionId);
    ChatQueueInfo GetInfo();
    void PingCurrentChats();
    void ChangeShifts();
}