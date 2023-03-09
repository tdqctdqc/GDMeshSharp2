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
    public void Draw(Data data, MapPolygon poly, PolyTri pt, Vector2 offset, IClient client)
    {
        Visible = true;
        Clear();
        Move(data, client, poly);
        var mb = new MeshBuilder();
        // DrawBoundarySegments(poly, mb, data);
        // DrawPolyBorderSegments(poly, mb, data);
        // DrawBoundaryTest(poly, mb, data);
        DrawNeighborBorders(poly, mb, data);
        DrawPolyTriBorders(poly, mb, data);
        TakeFromMeshBuilder(mb);
    }

    private void DrawNeighborBorders(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var edgeBorders = poly.GetNeighborEdgeSegments(data).ToList();
        mb.AddArrowsRainbow(edgeBorders, 5f);
        mb.AddNumMarkers(edgeBorders.Select(ls => ls.Mid()).ToList(), 20f, 
            Colors.Transparent, Colors.White, Vector2.Zero);
    }
    private void DrawBoundarySegments(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var lines = poly.GetBoundarySegments(data);
        mb.AddArrowsRainbow(lines.ToList(), 5f);
        mb.AddNumMarkers(lines.Select(ls => ls.Mid()).ToList(), 20f, 
            Colors.Transparent, Colors.White, Vector2.Zero);
    }

    private void DrawPolyTriBorders(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var col = Colors.Black;
        foreach (var t in poly.GetTerrainTris(data).Tris)
        {
            var inscribed = t.GetInscribed(.9f);
            mb.AddArrow(inscribed.A, inscribed.B, 1f, col);
            mb.AddArrow(inscribed.B, inscribed.C, 1f, col);
            mb.AddArrow(inscribed.C, inscribed.A, 1f, col);
        }
    }
    private void DrawPolyTriAndAdjacent(MapPolygon poly, PolyTri pt, MeshBuilder mb, Data data)
    {
        mb.AddTri(pt, Colors.White);
        // foreach (var n in poly.GetTerrainTris(data).NeighborsInside[pt])
        // {
        //     mb.AddTri(n, Colors.Red);
        // }
    }
    private void DrawPolyTriNetwork(MeshBuilder mb, MapPolygon poly, Data data)
    {
        var pts = poly.GetTerrainTris(data).Tris;
        foreach (var polyTri in pts)
        {
            // var ns = poly.GetTerrainTris(data).NeighborsInside[polyTri];
            // foreach (var n in ns)
            // {
            //     mb.AddArrow(polyTri.GetCentroid(), n.GetCentroid(), 1f, Colors.White);
            // }
        }
    }
    private void TakeFromMeshBuilder(MeshBuilder mb)
    {
        if (mb.Tris.Count == 0) return;
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