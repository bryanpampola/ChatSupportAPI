using ChatSupportAPI.Models;

namespace ChatSupportAPI.Services;

public interface IAgentCoordinatorService
{
    void AddAgents(List<Agent> agents);
    void AssignChat(ChatSession chat);
    void AssignChats(List<ChatSession>? chats);
    void UnassignChat(ChatSession chat);
    void UnassignChats(List<ChatSession> chats);
    int GetCapacity();
    List<string> GetInfo();
    void RemoveCurrentAgents();
}
