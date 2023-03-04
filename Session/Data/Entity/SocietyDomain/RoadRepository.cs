using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadRepository : Repository<RoadSegment>
{
    public RepoIndexer<RoadSegment, int> ByBorderId { get; private set; }
    public RoadRepository(Domain domain, Data data) : base(domain, data)
    {
        ByBorderId = new RepoIndexer<RoadSegment, int>(data, rs => rs.Border.RefId);
    }
}