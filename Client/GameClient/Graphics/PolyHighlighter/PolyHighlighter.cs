using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyHighlighter : Node2D
{
    private List<MeshInstance2D> _mis;

    public PolyHighlighter(Data data)
    {
        Game.I.Client.Requests.MouseOver.Subscribe(pos => Draw(data, pos));
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
    public void Draw(Data data, PolyTriPosition pos)
    {
        Visible = true;
        Clear();
        var poly = pos.Poly(data);
        var pt = pos.Tri(data);
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
        DrawPolyTriNetwork(poly, mb, data);
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
        foreach (var t in poly.Tris.Tris)
        {
            var inscribed = t.GetInscribed(.9f);
            mb.AddArrow(inscribed.A, inscribed.B, 1f, col);
            mb.AddArrow(inscribed.B, inscribed.C, 1f, col);
            mb.AddArrow(inscribed.C, inscribed.A, 1f, col);
        }
    }
    private static void DrawPolyTriNetwork(MapPolygon poly, MeshBuilder mb, Data data)
    {
        var pts = poly.Tris.Tris;
        foreach (var polyTri in pts)
        {
            for (var i = 0; i < polyTri.NeighborCount; i++)
            {
                var n = poly.Tris.TriNativeNeighbors[i + polyTri.NeighborStartIndex];
                var nTri = pts[n];
                mb.AddArrow(polyTri.GetCentroid(), nTri.GetCentroid(), 1f, Colors.White);
            }
        }
        
        foreach (var n in poly.Neighbors)
        {
            var offset = poly.GetOffsetTo(n, data);
            var edge = poly.GetEdge(n, data);
            var polyHi = edge.HighId.Entity() == poly;
            var pairs = polyHi
                ? edge.HiToLoTriPaths
                : edge.LoToHiTriPaths;
            foreach (var kvp in pairs)
            {
                var nativeTri = poly.Tris.Tris[kvp.Key];
                var foreignTri = n.Tris.Tris[kvp.Value];
                mb.AddArrow(nativeTri.GetCentroid(), foreignTri.GetCentroid() + offset,
                    1f, Colors.White);

            }
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