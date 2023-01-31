using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyHighlighter : Node2D
{
    private List<MeshInstance2D> _mis;
    public PolyHighlighter()
    {
        _mis = new List<MeshInstance2D>();
    }
    public void DrawOutline(MapPolygon poly, Vector2 offset, IClient client)
    {
        Clear();
        Move(client, poly);
        var mb = new MeshBuilder();
        var lines = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, client.Data))
            .SelectMany(b => b.GetSegsRel(poly));
        mb.AddLines(lines.ToList(), 20f, Colors.Pink);
        TakeFromMeshBuilder(mb);
    }

    public void DrawPolyTris(MapPolygon poly, IClient client)
    {
        Clear();
        Move(client, poly);
        var mb = new MeshBuilder();
        poly.GetTrisRel(client.Data).ForEach(t =>
        {
            mb.AddTri(t, Colors.Pink);
        });
        
        TakeFromMeshBuilder(mb);
    }


    public void SelectAspectTri<TAspect>(MapPolygon poly, Vector2 offset, IClient client)
        where TAspect : TerrainAspect
    {
        Clear();
        Move(client, poly);
        // var tris = client.Data.Models.GetManager<TAspect>()
        //     .GetTris(target).Tris;
        // if (tris.ContainsKey(poly.Id) == false) return;
        //
        // var polyTris = tris[poly.Id];
        // var hitTri = polyTris
        //     .FirstOrDefault(t => t.PointInsideTriangle(offset));
        // var mb = new MeshBuilder();
        // polyTris.ForEach(t => mb.AddTri(t, Colors.Yellow));
        // if(hitTri != null) mb.AddTri(hitTri, Colors.Red);
        // TakeFromMeshBuilder(mb);
    }
    
    public void SelectTargetAspectTri<TAspect>(TAspect target, MapPolygon poly, Vector2 offset, IClient client)
        where TAspect : TerrainAspect
    {
        Clear();
        Move(client, poly);
        var tris = client.Data.Planet.TerrainTris
            .GetTris(target).Tris;
        if (tris.ContainsKey(poly.Id) == false) return;
        
        var polyTris = tris[poly.Id];
        var hitTri = polyTris
            .FirstOrDefault(t => t.PointInsideTriangle(offset));
        var mb = new MeshBuilder();
        polyTris.ForEach(t => mb.AddTri(t, Colors.Yellow));
        if(hitTri != null) mb.AddTri(hitTri, Colors.Red);
        TakeFromMeshBuilder(mb);
    }
    
    public void DoXRay<TAspect>(MapPolygon poly, Vector2 offset, IClient client) where TAspect : TerrainAspect
    {
        Clear();
        Move(client, poly);
        var mb = new MeshBuilder();
        var manager = (TerrainAspectManager<TAspect>)client.Data.Models.GetManager<TAspect>();
        var aspects = manager.ByPriority;
        bool found = false;
        for (var i = aspects.Count - 1; i >= 0; i--)
        {
            var aspect = aspects[i];
            var aspectTris = client.Data.Planet.TerrainTris.GetTris(aspect).Tris;
            if (aspectTris.ContainsKey(poly.Id))
            {
                aspectTris[poly.Id].ForEach(t => mb.AddTri(t, ColorsExt.GetRainbowColor(i)));
                found = true;
            }
        }

        if (found == false)
        {
            // poly.tri
        }

        if (mb.Tris.Count > 0)
        {
            TakeFromMeshBuilder(mb);
        }
    }

    private void TakeFromMeshBuilder(MeshBuilder mb)
    {
        var mi = mb.GetMeshInstance();
        mb.Clear();
        AddChild(mi);
        _mis.Add(mi);
    }

    private void Move(IClient client, MapPolygon poly)
    {
        Position = client.Cam.GetMapPosInGlobalSpace(poly.Center, client.Data);
    }
    public void Clear()
    {
        
        _mis.ForEach(mi =>
        {
            RemoveChild(mi);
            mi?.QueueFree();
            mi = null;
        });
        _mis.Clear();
    }
}