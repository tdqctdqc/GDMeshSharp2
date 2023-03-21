using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonRepo : Repository<MapPolygon>
{
    public RepoEntityMultiIndexer<MapPolygon, Peep> PeepsInPoly { get; private set; }
    public IReadOnlyGraph<MapPolygon, PolyBorderChain> BorderGraph { get; private set; }
    public EntityValueCache<MapPolygon, PolyAuxData> AuxDatas { get; private set; }
    public PolyGrid MapPolyGrid { get; private set; }
    public HashSet<MapChunk> Chunks { get; private set; }
    public MapPolygonRepo(Domain domain, Data data) : base(domain, data)
    {
        BorderGraph = ImplicitGraph.Get<MapPolygon, PolyBorderChain>(
            () => Entities, 
            () => Entities.SelectMany(e => e.GetPolyBorders()).ToHashSet()
        );
        PeepsInPoly = new RepoEntityMultiIndexer<MapPolygon, Peep>(
            data,
            p => p.Home,
            nameof(Peep.Home)
        );
        AuxDatas = EntityValueCache<MapPolygon, PolyAuxData>.CreateTrigger(
            data,
            p => new PolyAuxData(p, data),
            this,
            data.Notices.SetPolyShapes, data.Notices.FinishedStateSync
        );
        data.Notices.SetPolyShapes.Subscribe(() => BuildPolyGrid(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildPolyGrid(data));
        
        data.Notices.SetPolyShapes.Subscribe(() => BuildChunks(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildChunks(data));
    }
    private void BuildPolyGrid(Data data)
    {
        var gridCellSize = 1000f;
        var numPartitions = Mathf.CeilToInt(data.Planet.PlanetInfo.Value.Dimensions.x / gridCellSize);
        MapPolyGrid = new PolyGrid(numPartitions, data.Planet.PlanetInfo.Value.Dimensions, data);
        foreach (var p in data.Planet.Polygons.Entities)
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
        foreach (var p in data.Planet.Polygons.Entities)
        {
            regularGrid.AddElement(p);
        }
        regularGrid.Update();
        Chunks = regularGrid.Cells.Select(c => c.Value)
            .Select(c => new MapChunk(c)).ToHashSet();
    }
}