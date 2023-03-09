
using System.Collections.Generic;

public class BorderChain<TSegment, TPrim, TRegion> : Chain<TSegment, TPrim>, IBorderChain<TSegment, TRegion>
    where TSegment : ISegment<TPrim>
{
    protected BorderChain(TRegion native, TRegion foreign, List<TSegment> segments) : base(segments)
    {
        Native = native;
        Foreign = foreign;
    }

    public TRegion Native { get; }
    public TRegion Foreign { get; }
}
