using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ITriBuilder
{
    List<Triangle> BuildTrisForPoly(MapPolygon p, WorldData data);
}