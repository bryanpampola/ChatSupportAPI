using ChatSupportAPI.Models;

namespace ChatSupportAPI.Services;

public interface IChatSupportEngine
{
    string StartChat(string name);
    Chat? GetChat(string sessionId);
    Chat? SendChat(string sessionId, string message);
    Chat? DisconnectChat(string sessionId);
    ChatQueueInfo GetInfo();
    void PingCurrentChats();
    void ChangeShifts();
}