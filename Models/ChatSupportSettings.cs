namespace ChatSupportAPI.Models;

public sealed class ChatSupportSettings
{
    public int PeriodicRun_InSeconds { get; set; }
    public ChatSettings ChatSettings { get; set; }
    public ChatQueueSettings ChatQueueSettings { get; set; }
    public ShiftSettings ShiftSettings { get; set; }
}

public class ChatSettings
{
    public int MaxRety { get; set; }
    public int RetryPolicy_InSeconds { get; set; }
    public int Expired_InSeconds { get; set; }
}

public class ChatQueueSettings
{
    public int CheckLive_InSeconds { get; set; }
    public int CheckExpired_InSeconds { get; set; }
}

public class ShiftSettings
{
    public bool IsAutoAssign { get; set; }
    public int CheckChange_InSeconds { get; set; }
}
