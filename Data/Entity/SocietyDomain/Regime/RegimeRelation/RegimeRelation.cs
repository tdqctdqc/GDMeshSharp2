using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RegimeRelation : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public EntityRef<Regime> HighId { get; private set; }
    public EntityRef<Regime> LowId { get; private set; }
    
    
    
    public static RegimeRelation Create(int id, CreateWriteKey key)
    {
        var rr = new RegimeRelation(id);
        key.Create(rr);
        return rr;
    }

    [SerializationConstructor] private RegimeRelation(int id) : base(id)
    {
    }
}