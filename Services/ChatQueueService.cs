using ChatSupportAPI.Models;
using Microsoft.Extensions.Options;

namespace ChatSupportAPI.Services;

public class ChatQueueService : IChatQueueService
{
    private Queue<ChatSession> QueueSessions { get; set; }
    private List<ChatSession> LiveSessions { get; set; }
    private int agentsCapacity = 0;
    private int maxThreshold = 0;

    private readonly ChatSettings _settings;

    public ChatQueueService(IOptions<ChatSupportSettings> settings)
    {
        _settings = settings.Value.ChatSettings;
        QueueSessions = new Queue<ChatSession>();
        LiveSessions = new List<ChatSession>();
    }

    public void SetAgentsCapacity(int capacity)
    {
        agentsCapacity = capacity;
        maxThreshold = Convert.ToInt32(capacity * 1.5);
    }
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
    public ChatSession? SendChat(string sesionId, string message)
    {
        var session = LiveSessions.FirstOrDefault(x => x.SessionId == sesionId);

        if (session != null)
        {
            session.AddMessage(message);
            session.Lifetime = DateTime.Now.AddSeconds(_settings.Expired_InSeconds);
            session.Retry = 0;
        }

        return session;
    }
    public ChatSession? EndSession(string sesionId)
    {
        var liveChat = LiveSessions.FirstOrDefault(x => x.SessionId == sesionId);
        if (liveChat != null)
        {
            LiveSessions.Remove(liveChat);
            return liveChat;
        }

        foreach (var queueChat in QueueSessions)
        {
            if (queueChat.SessionId == sesionId)
            {
                // set for delete later, since we cant remove on Queue
                queueChat.Retry = _settings.MaxRety;
                //break;
                return queueChat;
            }

        }

        return null;
    }
    public List<string> GetInfo()
    {
        var info = new List<string>();

        if (LiveSessions.Count > 0)
        {
            info.Add($"Live Chat Sessions | {LiveSessions.Count}");
            info.Add("-----------------------------------");
            info.AddRange(LiveSessions.Select(x => $"{x.SessionId}| {x.Name} <-> {x.AssignedAgent} | ({x.Retry}) {x.Lifetime:G}"));
            info.Add("-----------------------------------");
        }

        if (QueueSessions.Count > 0)
        {
            info.Add($"Queue Chat Sessions {QueueSessions.Count}");
            info.Add("-----------------------------------");
            info.AddRange(QueueSessions.Select(x => $"{x.SessionId}| {x.Name}"));
            info.Add("-----------------------------------");
        }

        return info;
    }
    public bool IsMaxCapacity()
    {
        return LiveSessions.Count + QueueSessions.Count >= maxThreshold;
    }
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
    public void CheckSessionRetryPolicyForTimeout()
    {
        foreach (var session in LiveSessions)
        {
            if (DateTime.Now <= session.Lifetime)
            {
                session.Retry += 1;
            }
        }
    }
    public List<ChatSession> ReturnExpiredSessions()
    {
        var expiredSessions = LiveSessions
            .Where(x => x.IsExpired() || x.Retry >= _settings.MaxRety);

        LiveSessions = LiveSessions.Except(expiredSessions).ToList();

        if (QueueSessions.TryPeek(out var nextSession) && nextSession.Retry > _settings.MaxRety)
        {
            QueueSessions.Dequeue();
        }

        return expiredSessions.ToList();
    }
    public List<ChatSession>? ReturnStartedQueueMessages()
    {
        var availableAgents = agentsCapacity - LiveSessions.Count;
        if (QueueSessions.Count == 0 || availableAgents <= 0)
        {
            return null;
        }
        
        var unassignedSessions = new List<ChatSession>();
        for (var i = 1; i <= QueueSessions.Count; i++)
        {
            if (i > availableAgents) break;
            if (QueueSessions.TryDequeue(out var session))
            {
                session.Lifetime = DateTime.Now.AddSeconds(_settings.Expired_InSeconds);
                session.AddMessage("Welcome, what can we do for you today?");
                LiveSessions.Add(session);
                unassignedSessions.Add(session);
            }
        }
        return unassignedSessions;
    }

}