using static ChatSupportAPI.Enums;

namespace ChatSupportAPI.Models;

public class Agent
{
    public string Name { get; set; }
    public string NickName { get; set; }
    public Seniority Seniority { get; set; }
    public int Capacity { get; private set; }
    public int CurrentChatCount { get; set; }
    public bool Assignable { get; set; }


    public Agent(string name, Seniority seniority)
    {
        Name = name;
        Seniority = seniority;
        Capacity = SetCapacity(seniority);
        Assignable = true;
    }

    private int SetCapacity(Seniority seniority)
    {
        switch (seniority)
        {
            case Seniority.Junior:
                return 4;
            case Seniority.MidLevel:
                return 8;
            case Seniority.Senior:
                return 6;
            case Seniority.TeamLead:
                return 5;
            default:
                return 4;
        }
    }

    public bool WithinCapacity()
    {
        return CurrentChatCount < Capacity;
    }
}