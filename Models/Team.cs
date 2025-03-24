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

}
