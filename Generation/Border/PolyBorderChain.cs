
    using System.Collections.Generic;
    using Godot;
    using MessagePack;

    public class PolyBorderChain : Chain<LineSegment, Vector2>, IBorderChain<LineSegment, MapPolygon>
    {
        public EntityRef<MapPolygon> Native { get; private set; }
        public EntityRef<MapPolygon> Foreign { get; private set; }
        public static PolyBorderChain Construct(MapPolygon native, MapPolygon foreign, List<LineSegment> segments)
        {
            return new PolyBorderChain(native.MakeRef(), foreign.MakeRef(), segments);
        }
        [SerializationConstructor] 
        private PolyBorderChain(EntityRef<MapPolygon> native, EntityRef<MapPolygon> foreign, List<LineSegment> segments) 
            : base(segments)
        {
            Native = native;
            Foreign = foreign;
        }

        MapPolygon IBorder<MapPolygon>.Native => Native.Entity();
        MapPolygon IBorder<MapPolygon>.Foreign => Foreign.Entity();
    }
