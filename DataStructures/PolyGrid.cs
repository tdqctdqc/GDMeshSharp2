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
        var key = new Vector2((int)(element.Center.x / _partitionSize.x),
            (int)(element.Center.y / _partitionSize.y));
        if(Cells.ContainsKey(key) == false)
        {
            Cells.Add(key, new List<MapPolygon>());
        }
        Cells[key].Add(element);
    }

    public void Update()
    {
        Cells = new Dictionary<Vector2, List<MapPolygon>>();
        foreach (var element in _data.Planet.Polygons.Entities)
        {
            UpdateElement(element);
        }
    }
    private void UpdateElement(MapPolygon element)
    {
        var newKey = new Vector2((int)(element.Center.x / _partitionSize.x),
            (int)(element.Center.y / _partitionSize.y));
        if(Cells.ContainsKey(newKey) == false) Cells.Add(newKey, new List<MapPolygon>());
        Cells[newKey].Add(element);
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
        Func<int, int, bool> grid = (int i, int j) =>
        {
            var offKey = new Vector2(i, j);
            if (CheckCell(point, offKey, out var p))
            {
                found = p;
                return false;
            }
            return true;
        };
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