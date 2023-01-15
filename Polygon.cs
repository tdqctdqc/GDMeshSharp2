using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Polygon : IGraphNode<Polygon, PolygonBorder>
{
    public int Id { get; private set; }
    public Vector2 Center { get; private set; }
    IReadOnlyList<Polygon> IGraphNode<Polygon, PolygonBorder>.Neighbors 
        => Neighbors;

    public PolygonBorder GetPolyBorder(Polygon neighbor) 
        => _borderDic[neighbor];
    public List<Polygon> Neighbors { get; private set; }
    public IEnumerable<PolygonBorder> NeighborBorders => Neighbors.Select(n => GetPolyBorder(n));
    private Dictionary<Polygon, PolygonBorder> _borderDic;
    public List<Vector2> NoNeighborBorders { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public Color Color { get; private set; }
    public Polygon(int id, Vector2 center, float mapWidth)
    {
        Id = id;
        Center = center;
        if (Center.x > mapWidth) Center = new Vector2(Center.x - mapWidth, center.y);
        if (Center.x < 0f) Center = new Vector2(Center.x + mapWidth, center.y);
        Neighbors = new List<Polygon>();
        NoNeighborBorders = new List<Vector2>();
        BoundingBox = new BoundingBox();
        _borderDic = new Dictionary<Polygon, PolygonBorder>();
        Color = ColorsExt.GetRandomColor();
    }
    
    public virtual void AddNeighbor(Polygon poly, PolygonBorder border)
    {
        if (Neighbors.Contains(poly)) return;
        Neighbors.Add(poly);
        _borderDic.Add(poly, border);
        var startN = Neighbors[0];
        for (int i = 0; i < Neighbors.Count; i++)
        {
            if (startN.Neighbors.Where(n => Neighbors.Contains(n)).Count() > 1)
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

        border.GetPointsAbs().ForEach(BoundingBox.RegisterPoint);
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
        BoundingBox.RegisterPoint(from);
        BoundingBox.RegisterPoint(to);
    }

    public List<Vector2> GetTrisAbs()
    {
        var tris = new List<Vector2>();
        for (var i = 0; i < Neighbors.Count; i++)
        {
            var edge = GetPolyBorder(Neighbors[i]);
            var segs = edge.GetSegsRel(this);
            for (var j = 0; j < segs.Count; j++)
            {
                tris.Add(Center);
                tris.Add(segs[j].From + Center);
                tris.Add(segs[j].To + Center);
            }
        }

        return tris;
    }
    public List<Vector2> GetTrisRel()
    {
        var tris = new List<Vector2>();
        for (var i = 0; i < Neighbors.Count; i++)
        {
            var edge = GetPolyBorder(Neighbors[i]);
            var segs = edge.GetSegsRel(this);
            for (var j = 0; j < segs.Count; j++)
            {
                tris.Add(Vector2.Zero);
                tris.Add(segs[j].From);
                tris.Add(segs[j].To);
            }
        }

        return tris;
    }

    public Vector2 GetOffsetTo(Polygon p, float mapWidth)
    {
        var off1 = p.Center - Center;
        var off2 = (off1 + Vector2.Right * mapWidth);
        var off3 = (off1 + Vector2.Left * mapWidth);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    
    public Vector2 GetOffsetTo(Vector2 p, float mapWidth)
    {
        var off1 = p - Center;
        var off2 = (off1 + Vector2.Right * mapWidth);
        var off3 = (off1 + Vector2.Left * mapWidth);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
}
