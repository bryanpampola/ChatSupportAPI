using ChatSupportAPI.Models;
using Microsoft.Extensions.Options;
using static ChatSupportAPI.Enums;

namespace ChatSupportAPI.Services;

public class ChatSupportEngine : IChatSupportEngine
{
    private List<ChatSession> ChatQueue { get; set; }
    private List<Agent> AgentList { get; set; }

    private int _teamCapacity;
    private int _queueCapacity;
    private WorkShift _currentWorkShift;

    private readonly int _chatRetryPolicyInSeconds;

    public ChatSupportEngine(IOptions<ChatSupportSettings> settings)
    {
        _chatRetryPolicyInSeconds = settings.Value.PeriodicRun_InSeconds;

        ChatQueue = new List<ChatSession>();

        // for testing purposes
        _currentWorkShift = WorkShift.Day;
        // uncomment this to get the correct workshift based on current time
        //_currentWorkShift = DateTime.Now.ToWorkShift();

        AgentList = Constants.Teams.First(x => x.Shift == _currentWorkShift)
            .Agents.OrderBy(x => x.Seniority).ToList();

        _teamCapacity = AgentList.Where(x => x.Assignable).Sum(x => x.Capacity);
        
        _queueCapacity = Convert.ToInt32(_teamCapacity * 1.5);
    }

    // polling process
    public void PingCurrentChats()
    {
        foreach (var chat in ChatQueue)
        {
            if (chat.Lifetime < DateTimeOffset.Now.AddSeconds(_chatRetryPolicyInSeconds))
            {
                chat.Retry += 1;
            }

            if (chat.Retry > 3)
            {
                ChatQueue.Remove(chat);
                AgentList.First(x => x.Name == chat.AssignedAgent).CurrentChatCount -= 1;
            }
        }

        foreach (var endShiftAgent in AgentList.Where(x => !x.Assignable).ToList())
        {
            if (endShiftAgent.CurrentChatCount < 1)
            {
                AgentList.Remove(endShiftAgent);
            }
        }
    }
    public void ChangeShifts()
    {
        // if same workshift, skip changeshift
        var workshiftNow = DateTime.Now.ToWorkShift();
        if (_currentWorkShift == workshiftNow)
        {
            return;
        }

        // change to new workshift
        _currentWorkShift = workshiftNow;
        var nextWorkShiftTeam = Constants.Teams.First(x => x.Shift == _currentWorkShift);

        var agentsWithOnGoingChats = AgentList.Where(x => x.CurrentChatCount > 0).ToList();
        foreach (var agent in agentsWithOnGoingChats)
        {
            agent.Assignable = false;
        }

        AgentList = new List<Agent>(nextWorkShiftTeam.Agents.Concat(agentsWithOnGoingChats));
        _teamCapacity = AgentList.Where(x => x.Assignable).Sum(x => x.Capacity);
        _queueCapacity = Convert.ToInt32(_teamCapacity * 1.5);

    }

    // stats
    public ChatQueueInfo GetInfo()
    {
        return new ChatQueueInfo
        {
            WorkShift = _currentWorkShift.ToString(),
            TeamCapacity = _teamCapacity,
            QueueCapacity = _queueCapacity,
            TotalAgents = AgentList.Count,
            TotalChatCount = ChatQueue.Count,
            OnWaitingQueue = ChatQueue.Count < _teamCapacity ? 0 : Convert.ToInt32(ChatQueue.Count - _teamCapacity),
            Chats = ChatQueue,
            Agents = AgentList
        };
    }

    // chat methods
    public string StartChat(string name)
    {
        if (ChatQueue.Count >= _queueCapacity)
        {
            return "The chat is refused.";
        }

        var chat = new ChatSession(name);

        chat.AssignedAgent = GetNextAvailableAgent();

        ChatQueue.Add(chat);

        return chat.SessionId;
    }
    public ChatSession SendChat(string sessionId, string message)
    {
        var chat = ChatQueue.FirstOrDefault(x => x.SessionId == sessionId);

        if (chat == null)
        {
            return chat;
        }

        if (chat?.Retry >= 3)
        {
            ChatQueue.Remove(chat);
        }

        chat.Retry = 0;
        chat.Lifetime = DateTime.Now;
        chat.Conversation.Add($"You: {message}");

        return chat;
    }
    public ChatSession? GetChat(string sessionId)
    {
        return ChatQueue.FirstOrDefault(x => x.SessionId == sessionId);
    }
    public ChatSession? DisconnectChat(string sessionId)
    {
        var chat = ChatQueue.FirstOrDefault(x => x.SessionId == sessionId);
        if (chat != null)
        {
            ChatQueue.Remove(chat);
            AgentList.First(x => x.Name == chat.AssignedAgent).CurrentChatCount -= 1;
        }
        return chat;
    }

    private string GetNextAvailableAgent()
    {
        var availableAgents = AgentList.Where(x => x.Assignable && x.WithinCapacity())
            .OrderBy(x => x.CurrentChatCount)
            .ThenBy(x => x.Seniority);

        if (availableAgents.Count() < 1)
        {
            return string.Empty;
        }

        var agent = availableAgents.First();
        agent.CurrentChatCount += 1;

        return agent.Name;
    }
}
