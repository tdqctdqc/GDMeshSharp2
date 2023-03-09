
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PolyBorderChain : Chain<LineSegment, Vector2>, IBorderChain<LineSegment, MapPolygon>
{
    public EntityRef<MapPolygon> Native { get; private set; }
    public EntityRef<MapPolygon> Foreign { get; private set; }
    public int RiverSegmentIndex { get; private set; }
    public static PolyBorderChain Construct(MapPolygon native, MapPolygon foreign, List<LineSegment> segments)
    {
        return new PolyBorderChain(native.MakeRef(), foreign.MakeRef(), segments, -1);
    }
    public static PolyBorderChain ConstructRiver(MapPolygon native, MapPolygon foreign, 
        List<LineSegment> segments, int riverIndex)
    {
        return new PolyBorderChain(native.MakeRef(), foreign.MakeRef(), segments, riverIndex);
    }
    [SerializationConstructor] 
    private PolyBorderChain(EntityRef<MapPolygon> native, EntityRef<MapPolygon> foreign, 
        List<LineSegment> segments, int riverSegmentIndex) 
        : base(segments)
    {
        RiverSegmentIndex = riverSegmentIndex;
        Native = native;
        Foreign = foreign;
    }

    public IEnumerable<LineSegment> SegsAbs()
    {
        return Segments.Select(ls => ls.Translate(Native.Entity().Center));
    }
    public LineSegment GetRiverSegment() => RiverSegmentIndex == -1 ? null : Segments[RiverSegmentIndex];
    public bool HasRiver() => RiverSegmentIndex != -1;
    MapPolygon IBorder<MapPolygon>.Native => Native.Entity();
    MapPolygon IBorder<MapPolygon>.Foreign => Foreign.Entity();
}
