
using System;

public class RegimeRelationRepo : Repository<RegimeRelation>
{
    public RepoEntityEdge<RegimeRelation, Regime> ByRegime { get; private set; }
    public RegimeRelationRepo(Domain domain, Data data) : base(domain, data)
    {
        ByRegime = new RepoEntityEdge<RegimeRelation, Regime>(data,
            r => r.Id, rr => new Tuple<Regime, Regime>(rr.HighId.Entity(), rr.LowId.Entity()));
    }
}
