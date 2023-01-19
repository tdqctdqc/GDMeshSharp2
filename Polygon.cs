using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Polygon : IGraphNode<Polygon, PolygonBorder>
{
    public int Id { get; private set; }
    public Vector2 Center { get; private set; }
    public List<Polygon> Neighbors { get; protected set; }
    protected Dictionary<Polygon, PolygonBorder> _borderDic;
    public List<Vector2> NoNeighborBorders { get; private set; }
    public Color Color { get; private set; }
    
    public bool HasNeighbor(Polygon p)
    {
        return _borderDic.ContainsKey(p);
    }
    public PolygonBorder GetPolyBorder(Polygon neighbor) 
        => _borderDic[neighbor];
    public Polygon(int id, Vector2 center, float mapWidth)
    {
        Id = id;
        Center = center;
        if (Center.x > mapWidth) Center = new Vector2(Center.x - mapWidth, center.y);
        if (Center.x < 0f) Center = new Vector2(Center.x + mapWidth, center.y);
        Neighbors = new List<Polygon>();
        NoNeighborBorders = new List<Vector2>();
        _borderDic = new Dictionary<Polygon, PolygonBorder>();
        Color = ColorsExt.GetRandomColor();
    }
    public IEnumerable<PolygonBorder> GetNeighborBorders() => Neighbors.Select(n => GetPolyBorder(n));

    public virtual void AddNeighbor(Polygon poly, PolygonBorder border)
    {
        if (Neighbors.Contains(poly)) return;
        Neighbors.Add(poly);
        _borderDic.Add(poly, border);
        var startN = Neighbors[0];
        for (int i = 0; i < Neighbors.Count; i++)
        {
            if (startN.Neighbors.Any(n => Neighbors.Contains(n)))
            {
                startN = Neighbors[(i + 1) % Neighbors.Count];
            }
            else break;
        }
        Neighbors = Neighbors
            .OrderByClockwise(Vector2.Zero, 
                n => GetPolyBorder(n).GetOffsetToOtherPoly(this),
                startN)
            .ToList();
    }

    public virtual void RemoveNeighbor(Polygon poly)
    {
        //only use in merging left-right wrap
        var index = Neighbors.IndexOf(poly);
        if (index == -1) return;
        Neighbors.RemoveAt(index);
        _borderDic.Remove(poly);
    }

    public void AddNoNeighborBorder(Vector2 from, Vector2 to)
    {
        NoNeighborBorders.Add(from);
        NoNeighborBorders.Add(to);
    }
    
    IReadOnlyList<Polygon> IGraphNode<Polygon, PolygonBorder>.Neighbors 
        => Neighbors;
}

public static class PolygonExt
{
    public static List<Vector2> GetTrisAbs(this Polygon p)
    {
        var tris = new List<Vector2>();
        for (var i = 0; i < p.Neighbors.Count; i++)
        {
            var edge = p.GetPolyBorder(p.Neighbors[i]);
            var segs = edge.GetSegsRel(p);
            for (var j = 0; j < segs.Count; j++)
            {
                tris.Add(p.Center);
                tris.Add(segs[j].From + p.Center);
                tris.Add(segs[j].To + p.Center);
            }
        }

        return tris;
    }
    public static List<Vector2> GetTrisRel(this Polygon p)
    {
        var tris = new List<Vector2>();
        for (var i = 0; i < p.Neighbors.Count; i++)
        {
            var edge = p.GetPolyBorder(p.Neighbors[i]);
            var segs = edge.GetSegsRel(p);
            for (var j = 0; j < segs.Count; j++)
            {
                tris.Add(Vector2.Zero);
                tris.Add(segs[j].From);
                tris.Add(segs[j].To);
            }
        }

        return tris;
    }

    public static Vector2 GetOffsetTo(this Polygon poly, Polygon p, float mapWidth)
    {
        var off1 = p.Center - poly.Center;
        var off2 = (off1 + Vector2.Right * mapWidth);
        var off3 = (off1 + Vector2.Left * mapWidth);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    
    public static Vector2 GetOffsetTo(this Polygon poly, Vector2 p, float mapWidth)
    {
        var off1 = p - poly.Center;
        var off2 = (off1 + Vector2.Right * mapWidth);
        var off3 = (off1 + Vector2.Left * mapWidth);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
}
