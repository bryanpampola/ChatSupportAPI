using ChatSupportAPI.Models;
using Microsoft.Extensions.Options;
using static ChatSupportAPI.Enums;

namespace ChatSupportAPI.Services;

public class ChatSupportService : IChatSupportService
{
    private readonly IChatQueueService _chatQueue;
    private readonly IChatQueueService _supportQueue;
    private readonly IAgentCoordinatorService _agentCoordinator;
    private readonly IAgentCoordinatorService _supportCoordinator;
    private readonly ChatSupportSettings _settings;

    private Team CurrentTeam { get; set; }
    private DateTime BackgroundProcess_QueueCheckLive_LastRun { get; set; }
    private DateTime BackgroundProcess_QueueCheckExpired_LastRun { get; set; }
    private DateTime BackgroundProcess_ShiftChange_LastRun { get; set; }

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
        _supportCoordinator = supportAgents;

        BackgroundProcess_QueueCheckLive_LastRun = DateTime.Now;
        BackgroundProcess_QueueCheckExpired_LastRun = DateTime.Now;
        BackgroundProcess_ShiftChange_LastRun = DateTime.Now;

        CurrentTeam = Enum.TryParse<WorkShift>(_settings.ShiftSettings.DefaultShift, out var shift) ? 
            shift.ToTeam() : WorkShift.Day.ToTeam();

        SetupQueuesAndShifts();

        _supportCoordinator.AddAgents(WorkShift.Custom.ToTeam().Agents);
        _supportQueue.SetAgentsCapacity(_supportCoordinator.GetCapacity());
    }

    public List<string> GetInfo()
    {
        var info = new List<string>();

        info.Add($" Team {CurrentTeam.Name} | {CurrentTeam.Shift}");
        info.AddRange(_agentCoordinator.GetInfo());
        info.AddRange(_chatQueue.GetInfo());

        if (CurrentTeam.Shift == WorkShift.Day)
        {
            info.Add($" Support Team - Day");
            info.AddRange(_supportCoordinator.GetInfo());
            info.AddRange(_supportQueue.GetInfo());
        }

        return info;
    }
    public string NewChatSession(string name)
    {
        var chat = _chatQueue.AddSession(name);
        if (chat != null && chat.Lifetime >= DateTime.Now)
        {
            _agentCoordinator.AssignChat(chat);
        }

        if (chat == null && 
            _chatQueue.IsMaxCapacity() &&
            CurrentTeam.Shift == WorkShift.Day)
        {
            chat = _supportQueue.AddSession(name);
            if (chat != null && chat.Lifetime >= DateTime.Now)
            {
                _supportCoordinator.AssignChat(chat);
            }
        }

        return chat?.SessionId ?? "Chat is refused. Please try again later.";
    }
    public bool EndChatSession(string sessionId)
    {
        var chat = _chatQueue.EndSession(sessionId);
        if (chat != null)
        {
            _agentCoordinator.UnassignChat(chat);
            return true;
        }

        var suppChat = _supportQueue.EndSession(sessionId);
        if (CurrentTeam.Shift == WorkShift.Day && suppChat != null)
        {
            _supportCoordinator.UnassignChat(suppChat);
            return true;
        }

        return false;
    }
    public ChatSession? SendChatMessage(string sessionId, string message)
    {
        var chat = _chatQueue.SendChat(sessionId, message);

        if (chat == null)
        {
            chat = _supportQueue.SendChat(sessionId, message);
        }

        return chat;
    }
    public void Utility_PingLiveSessions()
    {
        var lastRun = BackgroundProcess_QueueCheckLive_LastRun;
        if (DateTime.Now >= lastRun.AddSeconds(_settings.ChatQueueSettings.CheckLive_InSeconds))
        {
            _chatQueue.CheckSessionRetryPolicyForTimeout();
            _supportQueue.CheckSessionRetryPolicyForTimeout();

            BackgroundProcess_QueueCheckLive_LastRun = DateTime.Now;
        }
    }
    public void Utility_RemoveExpiredSessionOnQueue()
    {
        var lastRun = BackgroundProcess_QueueCheckExpired_LastRun;
        if (DateTime.Now >= lastRun.AddSeconds(_settings.ChatQueueSettings.CheckExpired_InSeconds))
        {
            var expiredChats = _chatQueue.ReturnExpiredSessions();
            _agentCoordinator.UnassignChats(expiredChats);

            var suppExpiredChats = _supportQueue.ReturnExpiredSessions();
            _supportCoordinator.UnassignChats(suppExpiredChats);

            BackgroundProcess_QueueCheckExpired_LastRun = DateTime.Now;
        }
    }
    public void Utility_AssignWaitingSessionOnQueue()
    {
        var unassignedChats = _chatQueue.ReturnStartedQueueMessages();
        _agentCoordinator.AssignChats(unassignedChats);

        var suppUnassignedChats = _supportQueue.ReturnStartedQueueMessages();
        _supportCoordinator.AssignChats(suppUnassignedChats);
    }
    public void Utility_ChangeTeamBasedOnWorkshift()
    {
        var lastRun = BackgroundProcess_ShiftChange_LastRun;
        if (DateTime.Now >= lastRun.AddSeconds(_settings.ShiftSettings.CheckChange_InSeconds))
        {
            if (_settings.ShiftSettings.IsAutoAssign && 
                CurrentTeam.Shift != DateTime.Now.ToWorkShift())
            {
                _agentCoordinator.SetAgentsUnassignable();
                CurrentTeam = DateTime.Now.ToWorkShift().ToTeam();
                SetupQueuesAndShifts();
                BackgroundProcess_ShiftChange_LastRun = DateTime.Now;
            }
            _agentCoordinator.RemoveUnassignableAgents();
        }
    }

    private void SetupQueuesAndShifts()
    {
        _agentCoordinator.AddAgents(CurrentTeam.Agents);
        _chatQueue.SetAgentsCapacity(_agentCoordinator.GetCapacity());
    }

}
