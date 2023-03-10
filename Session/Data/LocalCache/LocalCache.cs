using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LocalCache
{
    private Data _data;
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<MapPolygon, List<Triangle>> PolyRelWheelTris { get; private set; }
    public PolyGrid MapPolyGrid { get; private set; }
    public Dictionary<MapPolygon, List<LineSegment>> OrderedBoundarySegs { get; private set; }
    public Dictionary<MapPolygon, List<PolyBorderChain>> OrderedNeighborBorders { get; private set; }
    //todo sort out to repos?
    public LocalCache(Data data)
    {
        _data = data;
        if (data is GenData g)
        {
            g.Events.SetPolyShapes += () => SetPolyShapes(data);
        }
        else
        {
            data.Notices.FinishedStateSync += () => SetPolyShapes(data);
        }
    }


    private void SetPolyShapes(Data data)
    {
        OrderedNeighborBorders = data.Planet.Polygons.Entities
            .ToDictionary(p => p, p => p.GetPolyBorders().Ordered<PolyBorderChain, Vector2>().ToList());
        OrderedBoundarySegs = data.Planet.Polygons.Entities
            .ToDictionary(p => p, p => p.BuildBoundarySegments(data));
        BuildPolyGrid();
        BuildChunks(data);
        BuildPolyRelTris();
    }

    private void BuildPolyGrid()
    {
        var gridCellSize = 1000f;
        var numPartitions = Mathf.CeilToInt(_data.Planet.PlanetInfo.Value.Dimensions.x / gridCellSize);
        MapPolyGrid = new PolyGrid(numPartitions, _data.Planet.PlanetInfo.Value.Dimensions, _data);
        foreach (var p in _data.Planet.Polygons.Entities)
        {
            MapPolyGrid.AddElement(p);
        }
        MapPolyGrid.Update();
    }
    private void BuildChunks(Data data)
    {
        var regularGrid = new RegularGrid<MapPolygon>
        (
            polygon => polygon.Center,
            data.Planet.Width / 10f
        );
        foreach (var p in _data.Planet.Polygons.Entities)
        {
            regularGrid.AddElement(p);
        }
        regularGrid.Update();
        Chunks = regularGrid.Cells.Select(c => c.Value)
            .Select(c => new MapChunk(c)).ToHashSet();
    }

    private void BuildPolyRelTris()
    {
        PolyRelWheelTris = new Dictionary<MapPolygon, List<Triangle>>();
        foreach (var p in _data.Planet.Polygons.Entities)
        {
            var tris = new List<Triangle>();
            if (p.Neighbors.Refs().Count == 0)
            {
                throw new Exception();
            }
            foreach (var n in p.Neighbors.Refs())
            {
                var segs = p.GetBorder(n).Segments;
                for (var j = 0; j < segs.Count; j++)
                {
                    var tri = new Triangle(Vector2.Zero, segs[j].From, segs[j].To);
                    tris.Add(tri);
                }
            }
            PolyRelWheelTris.Add(p, tris);
        }
    }
}