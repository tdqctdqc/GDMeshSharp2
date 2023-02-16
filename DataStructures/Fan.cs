using System;
using System.Collections.Generic;
using Godot;

public class Fan
{
    public List<List<LineSegment>> Blades { get; private set; }
    public List<List<LineSegment>> Edges { get; private set; }

    public Fan(List<List<LineSegment>> blades, List<List<LineSegment>> edges)
    {
        Blades = blades;
        Edges = edges;
    }
}
