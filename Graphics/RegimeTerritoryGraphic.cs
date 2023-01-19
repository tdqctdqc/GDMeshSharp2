using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeTerritoryGraphic : Node2D
{
    public static Node2D Get(GenPlate plate, WorldData data)
    {
        var polys = plate.Cells.SelectMany(c => c.PolyGeos).ToList();
        var tris = new List<Vector2>();
        var colors = new List<Color>();
        for (var i = 0; i < polys.Count; i++)
        {
            var p = polys[i];
            var color = p.RegimeGen != null ? new Color(p.RegimeGen.PrimaryColor, .5f) : Colors.Transparent;
            
            var polyTriPoints = p.GetTrisRel().Select(v => v + plate.GetSeedPoly().GetOffsetTo(p, data.Dimensions.x));
            tris.AddRange(polyTriPoints);
            for (int j = 0; j < polyTriPoints.Count() / 3; j++)
            {
                colors.Add(color);
            }
        }
        var mesh = MeshGenerator.GetMeshInstance(tris.ToList(), colors);
        mesh.Position = plate.GetSeedPoly().Center;
        return mesh;
    }
}