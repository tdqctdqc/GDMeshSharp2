
public class ChooseDecisionCommand : Command
{
    public Decision Decision { get; private set; }
    public string Choice { get; private set; }

    public ChooseDecisionCommand(WriteKey key, Decision decision, string choice) : base(key)
    {
        Decision = decision;
        Choice = choice;
    }

    public override void Enact(HostWriteKey key)
    {
        Decision.PlayerEnact(Choice, key);
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}
