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

    public PolygonBorder GetEdge(Polygon neighbor) 
        => _borderDic[neighbor];
    public List<Polygon> Neighbors { get; private set; }
    public List<PolygonBorder> NeighborBorders { get; private set; }
    private Dictionary<Polygon, PolygonBorder> _borderDic;
    public List<Vector2> NoNeighborBorders { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public Polygon(int id, Vector2 center)
    {
        Id = id;
        Center = center;
        Neighbors = new List<Polygon>();
        NeighborBorders = new List<PolygonBorder>();
        NoNeighborBorders = new List<Vector2>();
        BoundingBox = new BoundingBox();
        _borderDic = new Dictionary<Polygon, PolygonBorder>();
    }
    
    public virtual void AddNeighbor(Polygon poly, PolygonBorder border)
    {
        if (Neighbors.Contains(poly)) return;
        Neighbors.Add(poly);
        NeighborBorders.Add(border);
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
        Neighbors = Neighbors.OrderByClockwise(Vector2.Zero, 
            n => GetEdge(n).GetOffsetToOtherPoly(this),
            startN)
            .ToList();
        NeighborBorders = Neighbors.Select(n => GetEdge(n)).ToList();

        border.GetPointsAbs().ForEach(BoundingBox.RegisterPoint);
    }

    public virtual void RemoveNeighbor(Polygon poly)
    {
        //only use in merging left-right wrap
        var index = Neighbors.IndexOf(poly);
        if (index == -1) return;
        Neighbors.RemoveAt(index);
        NeighborBorders.RemoveAt(index);
        _borderDic.Remove(poly);
    }

    public void AddNoNeighborBorder(Vector2 from, Vector2 to)
    {
        NoNeighborBorders.Add(from);
        NoNeighborBorders.Add(to);
        BoundingBox.RegisterPoint(from);
        BoundingBox.RegisterPoint(to);
    }

    public List<Vector2> GetTris()
    {
        var tris = new List<Vector2>();
        var borderPointLists =
            NeighborBorders
                .Select(b => b.GetPointsRel(this));
        int iter = 0;
        foreach (var bpList in borderPointLists)
        {
            for (var i = 0; i < bpList.Count - 1; i++)
            {
                iter++;
                tris.Add(bpList[i] + this.Center);
                tris.Add(bpList[i + 1] + this.Center);
                tris.Add(this.Center);
            }
        }

        return tris;
    }

    public List<Vector2> GetBorderPointsRelClockwise()
    {
        if (NoNeighborBorders.Count > 0)
        {
            return new List<Vector2>();
        }
        else
        {
            return NeighborBorders.SelectMany(n => n.GetPointsRel(this)).ToList();
        }
    }
}
