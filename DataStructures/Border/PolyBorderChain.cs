
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PolyBorderChain : Chain<LineSegment, Vector2>, IBorderChain<LineSegment, Vector2, MapPolygon>
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
        if(riverSegmentIndex > segments.Count - 1)
        {
            throw new Exception($"Bad segment index {riverSegmentIndex} out of {segments.Count}");
        }
        RiverSegmentIndex = riverSegmentIndex;
        Native = native;
        Foreign = foreign;
    }

    public IEnumerable<LineSegment> SegsAbs()
    {
        return Segments.Select(ls => ls.Translate(Native.Entity().Center));
    }
    public LineSegment GetRiverSegment() => HasRiver() ? Segments[RiverSegmentIndex] : null;
    public bool HasRiver() => RiverSegmentIndex != -1;
    MapPolygon IBorder<MapPolygon>.Native => Native.Entity();
    MapPolygon IBorder<MapPolygon>.Foreign => Foreign.Entity();
    public float PolyDist(Data data) => Native.Entity().GetOffsetTo(Foreign.Entity(), data).Length();
}
