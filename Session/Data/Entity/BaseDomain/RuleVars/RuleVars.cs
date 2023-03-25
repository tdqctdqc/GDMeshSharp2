using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RuleVars : Entity
{
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(RuleVars);
    public int FoodConsumptionPerPeep { get; private set; } = 10;
    public static RuleVars Create(GenWriteKey key)
    {
        var v = new RuleVars(key.IdDispenser.GetID());
        key.Create(v);
        return v;
    }
    [SerializationConstructor] private RuleVars(int foodConsumptionPerPeep, int id) : base(id)
    {
        FoodConsumptionPerPeep = foodConsumptionPerPeep;
    }

    private RuleVars(int id) : base(id)
    {
        
    }
    public override Type GetDomainType() => typeof(BaseDomain);
}
