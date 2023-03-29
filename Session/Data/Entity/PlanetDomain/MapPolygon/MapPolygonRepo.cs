using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonRepo : EntityAux<MapPolygon>
{
    public EntityMultiIndexer<MapPolygon, Peep> PeepsInPoly { get; private set; }
    public IReadOnlyGraph<MapPolygon, PolyBorderChain> BorderGraph { get; private set; }
    public EntityValueCache<MapPolygon, PolyAuxData> AuxDatas { get; private set; }
    public PolyGrid MapPolyGrid { get; private set; }
    public HashSet<MapChunk> Chunks { get; private set; }
    public LandSeaManager LandSea { get; private set; }

    public MapPolygonRepo(Domain domain, Data data) : base(domain, data)
    {
        BorderGraph = ImplicitGraph.Get<MapPolygon, PolyBorderChain>(
            () => Register.Entities, 
            () => Register.Entities.SelectMany(e => e.GetPolyBorders()).ToHashSet()
        );
        PeepsInPoly = new EntityMultiIndexer<MapPolygon, Peep>(
            data,
            p => p.Home,
            new RefAction[]{data.Notices.FinishedStateSync, data.Notices.PopulatedWorld},
            new RefAction<ValChangeNotice<EntityRef<MapPolygon>>>[]{}
        );
        AuxDatas = EntityValueCache<MapPolygon, PolyAuxData>.CreateTrigger(
            data,
            p => new PolyAuxData(p, data),
            data.Planet.Polygons,
            data.Notices.SetPolyShapes, data.Notices.FinishedStateSync
        );
        data.Notices.SetPolyShapes.Subscribe(() => BuildPolyGrid(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildPolyGrid(data));
        
        data.Notices.SetPolyShapes.Subscribe(() => BuildChunks(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildChunks(data));
        
        data.Notices.SetLandAndSea.Subscribe(() =>
        {
            LandSea = new LandSeaManager();
            LandSea.SetMasses(data);
        });
        data.Notices.FinishedStateSync.Subscribe(() =>
        {
            LandSea = new LandSeaManager();
            LandSea.SetMasses(data);
        });
    }
    private void BuildPolyGrid(Data data)
    {
        var gridCellSize = 1000f;
        var numPartitions = Mathf.CeilToInt(data.Planet.Info.Dimensions.x / gridCellSize);
        MapPolyGrid = new PolyGrid(numPartitions, data.Planet.Info.Dimensions, data);
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