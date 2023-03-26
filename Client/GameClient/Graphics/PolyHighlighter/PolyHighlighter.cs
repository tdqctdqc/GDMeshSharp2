using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyHighlighter : Node2D
{
    private List<MeshInstance2D> _mis;

    public PolyHighlighter(Data data)
    {
        Game.I.Client.Requests.MouseOver.Subscribe(pos => Draw(data, pos.Poly, pos.Tri));
        _mis = new List<MeshInstance2D>();
    }
    private PolyHighlighter()
    {
    }
    public enum Modes
    {
        Simple,
        Complex
    }
    public void Draw(Data data, MapPolygon poly, PolyTri pt)
    {
        Visible = true;
        Clear();
        Move(poly);
        var mb = new MeshBuilder();
        
        var mode = Game.I.Client.Settings.PolyHighlightMode.Value;
        if (mode == Modes.Simple)
        {
            DrawSimple(data, poly, pt, mb);
        }
        else if (mode == Modes.Complex)
        {
            DrawComplex(data, poly, pt, mb);
        }
        else throw new Exception();
        
        TakeFromMeshBuilder(mb);
    }

    private static void DrawSimple(Data data, MapPolygon poly, PolyTri pt, MeshBuilder mb)
    {
        DrawBordersSimple(poly, mb, data);
    }

    private static void DrawComplex(Data data, MapPolygon poly, PolyTri pt, MeshBuilder mb)
    {
        DrawBoundarySegments(poly, mb, data);
        DrawPolyTriBorders(poly, mb, data);
    }
    private static void DrawBordersSimple(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var edgeBorders = poly.GetOrderedNeighborSegments(data).Segments;
        mb.AddLines(edgeBorders, 10f, Colors.Black);
    }
    private static void DrawNeighborBorders(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var edgeBorders = poly.GetOrderedNeighborSegments(data).Segments;
        mb.AddArrowsRainbow(edgeBorders, 5f);
        mb.AddNumMarkers(edgeBorders.Select(ls => ls.Mid()).ToList(), 20f, 
            Colors.Transparent, Colors.White, Vector2.Zero);
    }
    private static void DrawBoundarySegments(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var lines = poly.GetOrderedBoundarySegs(data);
        mb.AddArrowsRainbow(lines.ToList(), 5f);
        mb.AddNumMarkers(lines.Select(ls => ls.Mid()).ToList(), 20f, 
            Colors.Transparent, Colors.White, Vector2.Zero);
    }
    private static void DrawPolyTriBorders(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var col = Colors.Black;
        foreach (var t in poly.TerrainTris.Tris)
        {
            var inscribed = t.GetInscribed(.9f);
            mb.AddArrow(inscribed.A, inscribed.B, 1f, col);
            mb.AddArrow(inscribed.B, inscribed.C, 1f, col);
            mb.AddArrow(inscribed.C, inscribed.A, 1f, col);
        }
    }
    private static void DrawPolyTriAndAdjacent(MapPolygon poly, PolyTri pt, MeshBuilder mb, Data data)
    {
        mb.AddTri(pt, Colors.White);
        // foreach (var n in poly.GetTerrainTris(data).NeighborsInside[pt])
        // {
        //     mb.AddTri(n, Colors.Red);
        // }
    }
    private static void DrawPolyTriNetwork(MeshBuilder mb, MapPolygon poly, Data data)
    {
        var pts = poly.TerrainTris.Tris;
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
    private void Move(MapPolygon poly)
    {
        Position = Game.I.Client.Cam.GetMapPosInGlobalSpace(poly.Center);
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