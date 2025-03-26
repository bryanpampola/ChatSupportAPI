using ChatSupportAPI.Models;

namespace ChatSupportAPI.Services;

public class AgentCoordinatorService : IAgentCoordinatorService
{
    private List<Agent> Agents { get; set; }

    public AgentCoordinatorService()
    {
        Agents = new List<Agent>();
    }

    public void AddAgents(List<Agent> agents)
    {
        Agents.AddRange(agents);
    }
    public void AssignChat(ChatSession chat)
    {
        AssignChats(new List<ChatSession> { chat });
    }
    public void AssignChats(List<ChatSession>? chats)
    {
        if (chats == null || chats.Count == 0) return;

        foreach (var chat in chats)
        {
            var availableAgent = GetNextAvailableAgent();
            if (availableAgent == null)
            {
                return;
            }

            chat.AssignedAgent = availableAgent.Name;
            chat.AddMessage($"Hi! This is {availableAgent.NickName}, ready to support you today!");
            availableAgent.Chats.Add(chat);
            availableAgent.CurrentChatCount += 1;
        }
    }
    public void UnassignChat(ChatSession chat)
    {
        UnassignChats(new List<ChatSession> { chat });
    }
    public void UnassignChats(List<ChatSession>? chats)
    {
        if (chats == null || chats.Count == 0) return;

        foreach (var chat in chats.Where(x => x.AssignedAgent != ""))
        {
            var agent = Agents.FirstOrDefault(x => x.Name == chat.AssignedAgent);

            if (agent == null) continue;

            agent.Chats.Remove(chat);
            agent.CurrentChatCount -= 1;
        }
    }
    public int GetCapacity()
    {
        return Agents.Where(x => x.Assignable).Sum(x => x.Capacity);
    }
    public List<string> GetInfo()
    {
        var info = new List<string>();

        if (Agents.Count > 0)
        {
            info.Add($"Support Agents | {Agents.Count}");
            info.Add("-----------------------------------");
            info.AddRange(Agents.Select(x => $"{x.NickName} - {x.Seniority}| On-going: {x.CurrentChatCount}"));
            info.Add("-----------------------------------");
        }

        return info;
    }
    public void SetAgentsUnassignable()
    {
        Agents.ForEach(x => x.Assignable = false);
    }
    public void RemoveUnassignableAgents()
    {
        if (Agents.All(x => x.Assignable)) return;

        var inactiveAgents = Agents.Where(x => !x.Assignable && x.CurrentChatCount == 0).ToList();

        Agents = Agents.Except(inactiveAgents).ToList();
    }
    
    private Agent? GetNextAvailableAgent()
    {
        var availableAgents = Agents.Where(x => x.Assignable && x.WithinCapacity())
            .OrderBy(x => x.CurrentChatCount)
            .ThenBy(x => x.Seniority);

        if (availableAgents.Count() < 1)
        {
            return null;
        }

        var agent = availableAgents.First();

        return agent;
    }

}
