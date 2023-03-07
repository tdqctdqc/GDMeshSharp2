
using System.Collections.Generic;

public class DecideAlliance : Decision
{
    public EntityRef<Regime> Offerer { get; private set; }
    
    public static DecideAlliance Create(Regime offerer, Regime decider)
    {
        return new DecideAlliance(offerer.MakeRef(), decider.MakeRef(), false);
    }
    private DecideAlliance(EntityRef<Regime> offerer, EntityRef<Regime> decider, bool decided) : base(decider, decided)
    {
        Offerer = offerer;
    }
    public override bool Valid(Data data)
    {
        if (Offerer.Check(data) && Decider.Check(data) == false) return false;
        var relation = Offerer.Entity().RelationWith(Decider.Entity(), data);
        if (relation.AtWar) return false;
        if (relation.Alliance) return false;
        return true;
    }

    public override string GetDescription()
    {
        return $"{Offerer.Entity().Name} is offering an alliance";
    }

    public override List<DecisionOption> GetOptions()
    {
        var accept = new DecisionOption(
            "Accept",
            "Accept",
            ai => 1f,
            k =>
            {
                var relation = Decider.Entity().RelationWith(Offerer.Entity(), k.Data);
                relation.Set(nameof(relation.Alliance), true, k);
            }
        );
        var reject = new DecisionOption(
            "Reject",
            "Reject",
            ai => 1f,
            k => { }
        );
        
        return new List<DecisionOption>{accept, reject};
    }
}
