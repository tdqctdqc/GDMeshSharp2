
using System;
using System.Collections.Generic;
using System.Linq;

public class Border<TSeg, TPrim, TRegion> 
    : IBorder<TSeg, TPrim, TRegion>, ISegment<TPrim> 
    where TSeg : ISegment<TPrim>
{
    public TRegion Native { get; private set; }
    public TRegion Foreign { get; private set; }
    public List<TSeg> Segments { get; private set; }
    

    public static Border<TSeg, TPrim, TRegion> Construct(TRegion native, 
        TRegion foreign, List<TSeg> elements)
    {
        return new Border<TSeg, TPrim, TRegion>(native, foreign, elements.OrderEndToStart());
    }
    protected Border(TRegion native, TRegion foreign, List<TSeg> segments)
    {
        Native = native;
        Foreign = foreign;
        Segments = segments;
    }

    public ISegment<TPrim> ReverseGeneric()
    {
        var r = Segments.Select(e => e.Reverse<TSeg, TPrim>()).ToList();
        r.Reverse();
        return new Border<TSeg, TPrim, TRegion>(Native, Foreign, r);
    }
    
    IReadOnlyList<TSeg> IBorder<TSeg, TPrim, TRegion>.Elements => Segments;
    TPrim ISegment<TPrim>.From => Segments[0].From;
    TPrim ISegment<TPrim>.To => Segments[Segments.Count - 1].To;
    object ISegment.From => Segments[0].From;
    object ISegment.To => Segments[Segments.Count - 1].To;
    ISegment ISegment.ReverseGeneric() => ReverseGeneric();
}
