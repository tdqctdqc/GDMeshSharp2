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
    public MapPolygon GetElementAtPoint(Vector2 point, out string msg)
    {
        int x = (int)(point.x / _partitionSize.x);
        int y = (int)(point.y / _partitionSize.y);
        var key = new Vector2(x,y);
        if (Cells.ContainsKey(key))
        {
            var first = Cells[key].FirstOrDefault(mp => mp.PointInPoly(point, _data));
            if (first != null)
            {
                msg = ("found on first");
                return first;
            }
            if (x == _partitionsPerAxis - 1)
            {
                var offKey = new Vector2(0, y);
                msg = ("found on wrap");

                var second = Cells[offKey].FirstOrDefault(mp => mp.PointInPoly(point, _data));
                if (second != null) return second;
            }
        }

        int iter = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0) continue;
                
                var offX = x + i;
                if (offX < 0) offX += _partitionsPerAxis;
                if (offX >= _partitionsPerAxis) offX -= _partitionsPerAxis;
                var offY = y + j;
                var offKey = new Vector2(offX, offY);
                if (Cells.ContainsKey(offKey))
                {
                    var first = Cells[offKey].FirstOrDefault(mp => mp.PointInPoly(point, _data));
                    if (first != null)
                    {
                        msg = ($"found on {iter++}th iteration");
                        return first;
                    }
                }
            }
        }

        msg = "Didnt find";
        return null;
    }
}