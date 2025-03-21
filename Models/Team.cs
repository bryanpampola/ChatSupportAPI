using static ChatSupportAPI.Enums;

namespace ChatSupportAPI.Models;

public sealed record Team
{
    public string Name { get; private set; }
    public WorkShift Shift { get; private set; }
    public List<Agent> Agents { get; private set; }

    public Team(string name, WorkShift shift, List<Agent> agents)
    {
        Name = name;
        Shift = shift;
        Agents = agents;
    }

    //public (DateTimeOffset ShiftStart, DateTimeOffset ShiftEnd) SetAgentShiftTimes()
    //{
    //    DateTime start, end;

    //    if (Shift == WorkShift.Day)
    //    {
    //        start = DateTime.Today.AddHours(8); // 8 AM
    //        end = DateTime.Today.AddHours(16); // 4 PM
    //    }
    //    else if (Shift == WorkShift.Evening)
    //    {
    //        start = DateTime.Today.AddHours(16); // 4 PM
    //        end = DateTime.Today.AddHours(24); // Midnight
    //    }
    //    else // Night shift
    //    {
    //        start = DateTime.Today.AddHours(24); // Midnight
    //        end = DateTime.Today.AddHours(8); // 8 AM (Next day)
    //    }
    //    return (start, end);
    //}
}
