
using System;
using System.Collections.Generic;

public interface ISegment<T>
{
    T From { get; }
    T To { get; }
    ISegment<T> Reverse();
    
}

public static class ISegmentExt
{
    public static bool ConnectsToStart<T>(this ISegment<T> seg, ISegment<T> connect)
    {
        return connect.To.Equals(seg.From);
    }

    public static bool ConnectsToEnd<T>(this ISegment<T> seg, ISegment<T> connect)
    {
        return connect.From.Equals(seg.To);
    }

    public static IBorder<T, TRegion> Sort<T, TRegion>(this IEnumerable<ISegment<T>> segs)
    {
        throw new NotImplementedException();
    }
}
