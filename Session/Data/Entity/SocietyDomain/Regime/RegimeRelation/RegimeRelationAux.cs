
using System;

public class RegimeRelationAux : EntityAux<RegimeRelation>
{
    public EntityEdge<RegimeRelation, Regime> ByRegime { get; private set; }
    public RegimeRelationAux(Domain domain, Data data) : base(domain, data)
    {
        ByRegime = new EntityEdge<RegimeRelation, Regime>(data,
            r => r.Id, rr => new Tuple<Regime, Regime>(rr.HighId.Entity(), rr.LowId.Entity()));
    }
}
