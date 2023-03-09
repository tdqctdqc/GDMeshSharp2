
    using System.Collections.Generic;
    using System.Linq;
    using Godot;
    using MessagePack;

    public class PolyBorderChain : Chain<LineSegment, Vector2>, IBorderChain<LineSegment, MapPolygon>
    {
        public EntityRef<MapPolygon> Native { get; private set; }
        public EntityRef<MapPolygon> Foreign { get; private set; }
        public static PolyBorderChain Construct(MapPolygon native, MapPolygon foreign, List<LineSegment> segments)
        {
            return new PolyBorderChain(native.MakeRef(), foreign.MakeRef(), segments, -1);
        }
        [SerializationConstructor] 
        private PolyBorderChain(EntityRef<MapPolygon> native, EntityRef<MapPolygon> foreign, 
            List<LineSegment> segments, int riverSegmentIndex) 
            : base(segments)
        {
            Native = native;
            Foreign = foreign;
        }

        public List<LineSegment> SegsAbs()
        {
            return Segments.Select(ls => ls.Translate(Native.Entity().Center)).ToList();
        }

        MapPolygon IBorder<MapPolygon>.Native => Native.Entity();
        MapPolygon IBorder<MapPolygon>.Foreign => Foreign.Entity();
    }
