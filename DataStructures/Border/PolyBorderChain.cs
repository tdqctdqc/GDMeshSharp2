
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PolyBorderChain : Chain<LineSegment, Vector2>, IBorderChain<LineSegment, Vector2, MapPolygon>
{
    public EntityRef<MapPolygon> Native { get; private set; }
    public EntityRef<MapPolygon> Foreign { get; private set; }
    public static PolyBorderChain Construct(MapPolygon native, MapPolygon foreign, List<LineSegment> segments)
    {
        return new PolyBorderChain(native.MakeRef(), foreign.MakeRef(), segments);
    }
    [SerializationConstructor] 
    private PolyBorderChain(EntityRef<MapPolygon> native, EntityRef<MapPolygon> foreign, 
        List<LineSegment> segments) 
        : base(segments)
    {
        Native = native;
        Foreign = foreign;
    }

    public IEnumerable<LineSegment> SegsAbs()
    {
        return Segments.Select(ls => ls.Translate(Native.Entity().Center));
    }
    MapPolygon IBorder<MapPolygon>.Native => Native.Entity();
    MapPolygon IBorder<MapPolygon>.Foreign => Foreign.Entity();
    public float PolyDist(Data data) => Native.Entity().GetOffsetTo(Foreign.Entity(), data).Length();
    
}
