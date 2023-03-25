using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class RegimeRelation : Entity
{
    public override Type GetDomainType() => typeof(SocietyDomain);
    public override Type GetRepoEntityType() => RepoEntityType();
    private static Type RepoEntityType() => typeof(RegimeRelation);
    public EntityRef<Regime> HighId { get; protected set; }
    public EntityRef<Regime> LowId { get; protected set; }
    public bool AtWar { get; protected set; }
    public bool OpenBorders { get; protected set; }
    public bool Alliance { get; protected set; }

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