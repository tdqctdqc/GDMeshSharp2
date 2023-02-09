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
    public void DrawOutline(Data data, MapPolygon poly, Vector2 offset, IClient client)
    {
        Visible = true;
        Clear();
        Move(data, client, poly);
        var mb = new MeshBuilder();
        var lines = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, data))
            .SelectMany(b => b.GetSegsRel(poly));
        mb.AddLines(lines.ToList(), 20f, Colors.Pink);
        TakeFromMeshBuilder(mb);
    }

    public void DrawPolyAndNeighbors(Data data, MapPolygon poly, IClient client)
    {
        Visible = true;
        Clear();
        Move(data, client, poly);
        var mb = new MeshBuilder();
        int iter = 0;
        poly.GetTrisRel(data).ForEach(t =>
        {
            mb.AddTri(t, ColorsExt.GetRainbowColor(iter++));
        });
        foreach (var n in poly.Neighbors.Refs())
        {
            n.GetTrisRel(data).ForEach(t =>
            {
                mb.AddTri(t.Transpose(poly.GetOffsetTo(n, data)), Colors.White);
            });
        }
        
        TakeFromMeshBuilder(mb);
    }
    public void DrawPolyTris(Data data, MapPolygon poly, IClient client)
    {
        Visible = true;
        Clear();
        Move(data, client, poly);
        var mb = new MeshBuilder();
        int iter = 0;
        poly.GetTrisRel(data).ForEach(t =>
        {
            mb.AddTri(t, ColorsExt.GetRainbowColor(iter++));
        });
        
        TakeFromMeshBuilder(mb);
    }


    public void SelectAspectTri<TAspect>(Data data, MapPolygon poly, Vector2 offset, IClient client)
        where TAspect : TerrainAspect
    {
        Visible = true;
        Clear();
        Move(data, client, poly);
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
    
    

    private void TakeFromMeshBuilder(MeshBuilder mb)
    {
        var mi = mb.GetMeshInstance();
        mb.Clear();
        AddChild(mi);
        _mis.Add(mi);
    }

    private void Move(Data data, IClient client, MapPolygon poly)
    {
        Position = client.Cam.GetMapPosInGlobalSpace(poly.Center, data);
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