using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RoadRepository : Repository<RoadSegment>
{
    public Dictionary<int, RoadSegment> ByBorderId { get; private set; }
    public RoadRepository(Domain domain, Data data) : base(domain, data)
    {
        ByBorderId = new Dictionary<int, RoadSegment>();
        AddedEntity += (segment, key) =>
        {
            ByBorderId.Add(segment.Border.RefId,  segment);
        };
        RemovingEntity += (segment, key) =>
        {
            ByBorderId.Remove(segment.Border.RefId);
        };
    }
}