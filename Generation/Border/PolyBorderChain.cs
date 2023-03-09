
    using System.Collections.Generic;
    using Godot;

    public class PolyBorderChain : BorderChain<LineSegment, Vector2, MapPolygon>
    {
        public PolyBorderChain(MapPolygon native, MapPolygon foreign, List<LineSegment> segments) 
            : base(native, foreign, segments)
        {
        }
    }
