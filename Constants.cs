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
            new Agent("TeamLeadA", Seniority.TeamLead, "Andrew"),
            new Agent("MidLevelA1", Seniority.MidLevel, "Alisha"),
            new Agent("MidLevelA2", Seniority.MidLevel, "Anna"),
            new Agent("JuniorA", Seniority.Junior, "Albert")
        };
        public static List<Agent> TeamB => new List<Agent>
        {
            new Agent("SeniorB", Seniority.Senior, "Bryan"),
            new Agent("MidLevelB", Seniority.MidLevel, "Ben"),
            new Agent("JuniorB1", Seniority.Junior, "Belinda"),
            new Agent("JuniorB2", Seniority.Junior, "Brenda")
        };
        public static List<Agent> TeamC => new List<Agent>
        {
            new Agent("MidLevelC1", Seniority.MidLevel, "Catherine"),
            new Agent("MidLevelC2", Seniority.MidLevel, "Charles")
        };
        public static List<Agent> TeamOverflow => new List<Agent>
        {
            new Agent("Overflow1", Seniority.Junior, "O-One"),
            new Agent("Overflow2", Seniority.Junior, "O-Two"),
            new Agent("Overflow3", Seniority.Junior, "O-Three"),
            new Agent("Overflow4", Seniority.Junior, "O-Four"),
            new Agent("Overflow5", Seniority.Junior, "O-Five"),
            new Agent("Overflow6", Seniority.Junior, "O-Six")
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

    public static Team ToTeam(this WorkShift workShift) 
        => Constants.Teams.First(x => x.Shift == workShift);

}
