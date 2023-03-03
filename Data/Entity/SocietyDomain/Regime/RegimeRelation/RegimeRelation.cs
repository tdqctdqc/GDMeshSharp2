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
    public bool AtWar { get; private set; }
    public bool OpenBorders { get; private set; }
    public bool Alliance { get; private set; }

    [SerializationConstructor] private RegimeRelation(int id, EntityRef<Regime> lowId, EntityRef<Regime> highId,
        bool atWar, bool openBorders, bool alliance) : base(id)
    {
        AtWar = atWar;
        OpenBorders = openBorders;
        Alliance = alliance;
        if (lowId.RefId == highId.RefId) throw new Exception();
        HighId = lowId.RefId > highId.RefId ? lowId : highId;
        LowId = lowId.RefId > highId.RefId ? highId : lowId;
    }

    public static RegimeRelation Create(int id, EntityRef<Regime> r1, EntityRef<Regime> r2, CreateWriteKey key)
    {
        var rr = new RegimeRelation(id, r1, r2, false, false, false);
        key.Create(rr);
        return rr;
    }
}