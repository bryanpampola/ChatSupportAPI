using ChatSupportAPI.Models;
using static ChatSupportAPI.Enums;

namespace ChatSupportAPI;

public static class Constants
{
    public static List<Team> Teams => new List<Team>
    {
        new Team("Team A - (8am-4pm)", WorkShift.Day, DefaultAgents.TeamA),
        new Team("Team B - (4pm-12am)", WorkShift.Evening, DefaultAgents.TeamB),
        new Team("Team C - (12am-8am)", WorkShift.Night, DefaultAgents.TeamC),
        new Team("Team Overflow - (8am-4pm)", WorkShift.Custom, DefaultAgents.TeamOverflow),
    };

    public static class DefaultAgents
    {
        
        public static List<Agent> TeamA => new List<Agent>
        {
            new Agent("TeamLeadA", Seniority.TeamLead),
            new Agent("MidLevelA1", Seniority.MidLevel),
            new Agent("MidLevelA2", Seniority.MidLevel),
            new Agent("JuniorA", Seniority.Junior)
        };
        public static List<Agent> TeamB => new List<Agent>
        {
            new Agent("SeniorB", Seniority.Senior),
            new Agent("MidLevelB", Seniority.MidLevel),
            new Agent("JuniorB1", Seniority.Junior),
            new Agent("JuniorB2", Seniority.Junior)
        };
        public static List<Agent> TeamC => new List<Agent>
        {
            new Agent("MidLevelC1", Seniority.MidLevel),
            new Agent("MidLevelC2", Seniority.MidLevel)
        };
        public static List<Agent> TeamOverflow => new List<Agent>
        {
            new Agent("Overflow1", Seniority.Junior),
            new Agent("Overflow2", Seniority.Junior),
            new Agent("Overflow3", Seniority.Junior),
            new Agent("Overflow4", Seniority.Junior),
            new Agent("Overflow5", Seniority.Junior),
            new Agent("Overflow6", Seniority.Junior)
        };
    }
}

public static class Enums
{
    public enum Seniority
    {
        Junior = 1,
        MidLevel = 2,
        Senior = 3,
        TeamLead = 4
    }

    public enum WorkShift
    {
        Day,
        Evening,
        Night,
        Custom
    }
}

public static class Extensions
{
    public static WorkShift ToWorkShift(this DateTime date)
    {
        if (date >= DateTime.Today.AddHours(8) && date <= DateTime.Today.AddHours(16))
        {
            return WorkShift.Day;
        } 
        else if (date >= DateTime.Today.AddHours(16) && date <= DateTime.Today.AddHours(24))
        {
            return WorkShift.Evening;
        }
        else
        {
            return WorkShift.Evening;
        }
    }
}
