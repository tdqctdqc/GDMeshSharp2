using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadRepository : Repository<RoadSegment>
{
    public RepoEntityIndexer<RoadSegment, MapPolygonEdge> ByEdgeId { get; private set; }
    public RoadRepository(Domain domain, Data data) : base(domain, data)
    {
        ByEdgeId = RepoEntityIndexer<RoadSegment, MapPolygonEdge>.CreateStatic(data, rs => rs.Edge);
    }
}