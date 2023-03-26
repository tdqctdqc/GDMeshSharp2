using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RuleVars : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(BaseDomain);
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(RuleVars);
    public int FoodConsumptionPerPeep { get; protected set; }
    public static RuleVars CreateDefault(GenWriteKey key)
    {
        var v = new RuleVars(
            10,
            key.IdDispenser.GetID());
        key.Create(v);
        return v;
    }
    [SerializationConstructor] private RuleVars(int foodConsumptionPerPeep, int id) : base(id)
    {
        FoodConsumptionPerPeep = foodConsumptionPerPeep;
    }
    
}
