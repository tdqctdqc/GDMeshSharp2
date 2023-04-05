
using System;

public class ChooseDecisionCommand : Command
{
    public Decision Decision { get; private set; }
    public string Choice { get; private set; }

    public ChooseDecisionCommand(Decision decision, string choice) : base()
    {
        Decision = decision;
        Choice = choice;
    }

    public override void Enact(HostWriteKey key, Action<Procedure> queueProc)
    {
        Decision.PlayerEnact(Choice, key);
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}
