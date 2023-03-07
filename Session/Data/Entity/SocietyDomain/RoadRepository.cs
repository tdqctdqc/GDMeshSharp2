using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadRepository : Repository<RoadSegment>
{
    public RepoEntityIndexer<RoadSegment, MapPolygonBorder> ByBorderId { get; private set; }
    public RoadRepository(Domain domain, Data data) : base(domain, data)
    {
        ByBorderId = RepoEntityIndexer<RoadSegment, MapPolygonBorder>.CreateStatic(data, rs => rs.Border);
    }
}