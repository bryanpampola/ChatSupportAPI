using ChatSupportAPI.Models;
using Microsoft.Extensions.Options;
using static ChatSupportAPI.Enums;

namespace ChatSupportAPI.Services;

public interface IChatQueueService
{
    void SetAgentsCapacity(int capacity);
    ChatSession? AddSession(string name);
    ChatSession? SendChat(string sesionId, string message);
    bool EndSession(string sesionId);
    List<string> GetInfo();
    bool IsMaxCapacity();
}

public class ChatQueueService : IChatQueueService
{
    private Queue<ChatSession> QueueSessions { get; set; }
    private List<ChatSession> LiveSessions { get; set; }
    private int agentsCapacity = 0;
    private int maxThreshold = 0;

    private readonly ChatSettings _settings; //= new ChatSettings();

    public ChatQueueService(IOptions<ChatSupportSettings> settings)
    {
        _settings = settings.Value.ChatSettings;
        QueueSessions = new Queue<ChatSession>();
        LiveSessions = new List<ChatSession>();
    }

    // set agents capacity -- set max cap of que
    public void SetAgentsCapacity(int capacity)
    {
        agentsCapacity = capacity;
        maxThreshold = Convert.ToInt32(capacity * 1.5);
    }

    // add session -- add session on live or queue
    public ChatSession? AddSession(string name)
    {
        var session = new ChatSession(name);

        if (LiveSessions.Count < agentsCapacity)
        {
            session.Lifetime = DateTime.Now.AddSeconds(_settings.Expired_InSeconds);
            session.AddMessage("Welcome, what can we do for you today?");
            LiveSessions.Add(session);
            return session;
        }

        if (LiveSessions.Count + QueueSessions.Count < maxThreshold)
        {
            session.AddMessage("Please wait as soon as our available agent will contact you.");
            QueueSessions.Enqueue(session);
            return session;
        }

        return null;
    }

    // send chat -- extend session
    public ChatSession? SendChat(string sesionId, string message)
    {
        var chat = LiveSessions.FirstOrDefault(x => x.SessionId == sesionId);

        if (chat != null)
        {
            chat.AddMessage(message);
        }

        return chat;
    }

    // end session -- remove session on que
    public bool EndSession(string sesionId)
    {
        var liveChat = LiveSessions.FirstOrDefault(x => x.SessionId == sesionId);
        if (liveChat != null)
        {
            LiveSessions.Remove(liveChat);
            return true;
        }

        foreach (var queueChat in QueueSessions)
        {
            if (queueChat.SessionId == sesionId)
            {
                // set for delete later, since we cant remove on Queue
                queueChat.Retry = _settings.MaxRety;
                //break;
                return true;
            }

        }

        return false;
    }  

    // get info
    public List<string> GetInfo()
    {
        var info = new List<string>();

        if (LiveSessions.Count > 0)
        {
            info.Add($"Live Chat Sessions | {LiveSessions.Count}");
            info.Add("----------------------------");
            info.AddRange(LiveSessions.Select(x => $"{x.SessionId}| {x.Name}"));
            info.Add("----------------------------");
        }

        if (QueueSessions.Count > 0)
        {
            info.Add($"Queue Chat Sessions {QueueSessions.Count}");
            info.Add("----------------------------");
            info.AddRange(QueueSessions.Select(x => $"{x.SessionId}| {x.Name}"));
            info.Add("----------------------------");
        }

        return info;
    }

    public bool IsMaxCapacity()
    {
        return LiveSessions.Count + QueueSessions.Count >= maxThreshold;
    }

    // start que sessions -- check queue, get que, start live
    public ChatSession? StartQueuedSessions()
    {
        if (!QueueSessions.TryDequeue(out var nextSession) || nextSession.Retry > _settings.MaxRety)
        {
            return null;
        }

        nextSession.Lifetime = DateTime.Now.AddSeconds(_settings.Expired_InSeconds);
        nextSession.AddMessage("Welcome, what can we do for you today?");

        LiveSessions.Add(nextSession);

        return nextSession;
    }

    // POLLING | ping live sessions -- increase retry based on policy
    public void PingLiveSessions()
    {
        foreach (var session in LiveSessions)
        {
            if (DateTime.Now.AddSeconds(_settings.RetryPolicy_InSeconds) >= DateTime.Now)
            {
                session.Retry += 1;
            }
        }
    }

    // POLLING | remove expired sessions
    public List<string> RemovedExpiredSessions()
    {
        var expiredSessions = LiveSessions.Where(x => x.IsExpired() || x.Retry > _settings.MaxRety)
            .Select(x => x.SessionId)
            .ToList();

        foreach (var session in LiveSessions.Where(x => expiredSessions.Contains(x.SessionId)))
        {
            LiveSessions.Remove(session);
        }

        if (QueueSessions.TryPeek(out var nextSession) && nextSession.Retry > _settings.MaxRety)
        {
            QueueSessions.Dequeue();
        }

        return expiredSessions;
    }
}


public interface IAgentCoordinatorService
{
    void AddAgents(List<Agent> agents);
    void AssignChat(ChatSession? session);
    void UnassignChat(string sessionId);
    int GetCapacity();
    List<string> GetInfo();
}

public class AgentCoordinatorService : IAgentCoordinatorService
{
    private List<Agent> Agents { get; set; }

    public AgentCoordinatorService()
    {
        Agents = new List<Agent>();
    }

    // add agents -- add agents on list, compute max capacity
    public void AddAgents(List<Agent> agents)
    {
        Agents.AddRange(agents);
    }

    // assign agent -- add 1 to agent cap
    public void AssignChat(ChatSession? session)
    {
        var availableAgent = GetNextAvailableAgent();
        if (session == null || availableAgent == null)
        {
            return;
        }

        availableAgent.Chats.Add(session);
        availableAgent.CurrentChatCount += 1;
    }

    // unassign chat -- remove chat from agent
    public void UnassignChat(string sessionId)
    {
        foreach (var agent in Agents.Where(x => x.CurrentChatCount > 0))
        {
            if (agent.Chats.Any(x => x.SessionId == sessionId))
            {
                agent.Chats.Remove(agent.Chats.First(x => x.SessionId == sessionId));
                agent.CurrentChatCount -= 1;
                break;
            }
        }
    }

    // get cap -- team cap
    public int GetCapacity()
    {
        return Agents.Where(x => x.Assignable).Sum(x => x.Capacity);
    }

    // get info
    public List<string> GetInfo()
    {
        var info = new List<string>();

        if (Agents.Count > 0)
        {
            info.Add($"Support Agents | {Agents.Count}");
            info.Add("----------------------------");
            info.AddRange(Agents.Select(x => $"{x.NickName} - {x.Seniority}| On-going: {x.CurrentChatCount}"));
            info.Add("----------------------------");
        }

        return info;
    }
    
    // POLLING | remove current agents for shift change
    public void RemoveCurrentAgents()
    {
        var agentsWithOnGoingChats = Agents.Where(x => x.CurrentChatCount > 0).ToList();
        Agents.Clear();

        if (agentsWithOnGoingChats.Count == 0)
        {
            return;
        }

        agentsWithOnGoingChats.ForEach(x => x.Assignable = false);
        Agents.AddRange(agentsWithOnGoingChats);
    }

    // get available agent -- get agent with cap, round-robin based on seniority
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
        agent.CurrentChatCount += 1;

        return agent;
    }
}

public interface IChatSupportService
{
    string NewChatSession(string name);
    ChatSession? SendMessage(string sessionId, string message);
    bool EndChatSession(string sessionId);
    List<string> GetInfo();

}

public class ChatSupportService : IChatSupportService
{
    private readonly IChatQueueService _chatQueue;
    private readonly IChatQueueService _supportQueue;
    private readonly IAgentCoordinatorService _agentCoordinator;
    private readonly IAgentCoordinatorService _supportAgents;
    private readonly ChatSupportSettings _settings;

    private Team _currentTeam;

    public ChatSupportService(IOptions<ChatSupportSettings> settings,
        IChatQueueService chatQueue,
        IAgentCoordinatorService agentCoordinator,
        IChatQueueService supportQueue,
        IAgentCoordinatorService supportAgents)
    {
        _settings = settings.Value;
        _chatQueue = chatQueue;
        _agentCoordinator = agentCoordinator;
        _supportQueue = supportQueue;
        _supportAgents = supportAgents;
        Initialize();
    }

    private void Initialize()
    {
        _currentTeam = GetTeam(_settings.ShiftSettings.IsAutoAssign);
        _agentCoordinator.AddAgents(_currentTeam.Agents);
        _chatQueue.SetAgentsCapacity(_agentCoordinator.GetCapacity());
        
        if (_currentTeam.Shift == WorkShift.Day)
        {
            _supportAgents.AddAgents(WorkShift.Custom.ToTeam().Agents);
            _supportQueue.SetAgentsCapacity(_supportAgents.GetCapacity());
        }
    }

    private Team GetTeam(bool isAutoAssign)
    {
        return !isAutoAssign ? WorkShift.Day.ToTeam() : DateTime.Now.ToWorkShift().ToTeam(); 
    }

    public string NewChatSession(string name)
    {
        var chat = _chatQueue.AddSession(name);
        _agentCoordinator.AssignChat(chat);

        if (chat == null && 
            _chatQueue.IsMaxCapacity() &&
            _currentTeam.Shift == WorkShift.Day)
        {
            chat = _supportQueue.AddSession(name);
            _supportAgents.AssignChat(chat);
        }

        return chat?.SessionId ?? "Chat is refused. Please try again later.";
    }

    public bool EndChatSession(string sessionId)
    {
        if (_chatQueue.EndSession(sessionId))
        {
            _agentCoordinator.UnassignChat(sessionId);
            return true;
        }

        if (_currentTeam.Shift == WorkShift.Day && _chatQueue.EndSession(sessionId))
        {
            _supportAgents.UnassignChat(sessionId);
            return true;
        }

        return false;
    }

    public List<string> GetInfo()
    {
        var info = new List<string>();

        info.Add($" Team {_currentTeam.Name} | {_currentTeam.Shift}");
        info.AddRange(_agentCoordinator.GetInfo());
        info.AddRange(_chatQueue.GetInfo());
        
        if (_currentTeam.Shift == WorkShift.Day)
        {
            info.Add($" Support Team - Day");
            info.AddRange(_supportAgents.GetInfo());
            info.AddRange(_supportQueue.GetInfo());
        }

        return info;
    }

    public ChatSession? SendMessage(string sessionId, string message)
    {
        return _chatQueue.SendChat(sessionId, message);
    }
}
