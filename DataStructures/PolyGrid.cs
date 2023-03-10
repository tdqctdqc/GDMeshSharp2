using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyGrid
{
    public Dictionary<Vector2, List<MapPolygon>> Cells;
    private Vector2 _partitionSize;
    private int _partitionsPerAxis;
    private Data _data;
    
    public PolyGrid(int numPartitionsPerAxis, Vector2 dim, Data data)
    {
        _partitionsPerAxis = numPartitionsPerAxis;
        _data = data;
        _partitionSize = dim / numPartitionsPerAxis;
        Cells = new Dictionary<Vector2, List<MapPolygon>>();
    }
    public void AddElement(MapPolygon element)
    {
        var borderPoints = element.GetOrderedBoundarySegs(_data).GetPoints().ToHashSet();
        var minX = borderPoints.Min(p => p.x);
        var minXCoord = Mathf.FloorToInt(minX / _partitionSize.x);
        var maxX = borderPoints.Max(p => p.x);
        var maxXCoord = Mathf.CeilToInt(maxX / _partitionSize.x);

        var minY = borderPoints.Min(p => p.y);
        var minYCoord = Mathf.FloorToInt(minY / _partitionSize.y);

        var maxY = borderPoints.Max(p => p.y);
        var maxYCoord = Mathf.CeilToInt(maxY / _partitionSize.y);

        for (int i = minXCoord; i <= maxXCoord; i++)
        {
            for (int j = minYCoord; j < maxYCoord; j++)
            {
                var key = new Vector2(i, j);
                if(Cells.ContainsKey(key) == false)
                {
                    Cells.Add(key, new List<MapPolygon>());
                }
                Cells[key].Add(element);
            }
        }
        
    }

    public void Update()
    {
        Cells = new Dictionary<Vector2, List<MapPolygon>>();
        foreach (var element in _data.Planet.Polygons.Entities)
        {
            AddElement(element);
        }
    }
    public MapPolygon GetElementAtPoint(Vector2 point)
    {
        int x = (int)(point.x / _partitionSize.x);
        int y = (int)(point.y / _partitionSize.y);
        var key = new Vector2(x,y);
        if (CheckCell(point, key, out var mp1))
        {
            return mp1;
        }
        if (x == _partitionsPerAxis - 1)
        {
            var offKey = new Vector2(0, y);
            if (CheckCell(point, offKey, out var mp2))
            {
                return mp2;
            }
        }

        MapPolygon found = null;
        EnumerableExt.DoForGridAround(
            (int i, int j) =>
            {
                var offKey = new Vector2(i, j);
                if (CheckCell(point, offKey, out var p))
                {
                    found = p;
                    return false;
                }
                return true;
            }, x, y
        );
        return found;
    }


    private bool CheckCell(Vector2 point, Vector2 key, out MapPolygon p)
    {
        if (Cells.TryGetValue(key, out var cell))
        {
            p = cell.FirstOrDefault(mp => mp.PointInPoly(point, _data));
            if (p != null)
            {
                return true;
            }
        }

        p = null;
        return false;
    }
}