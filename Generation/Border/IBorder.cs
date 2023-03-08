
using System.Collections.Generic;

public interface IBorder<TBorderPrimitive, TRegion>
{
    TRegion Native { get; }
    TRegion Foreign { get; }
    IReadOnlyList<ISegment<TBorderPrimitive>> Elements { get; }
}

public static class IBorderExt
{
    public static ISegment<TSegment> First<TSegment, TRegion>(this IBorder<TSegment, TRegion> border)
    {
        return border.Elements[0];
    }
    public static ISegment<T> Last<T, TRegion>(this IBorder<T, TRegion> border)
    {
        return border.Elements[border.Elements.Count - 1];
    }
}
