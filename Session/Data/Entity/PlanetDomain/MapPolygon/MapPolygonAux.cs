using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolygonAux : EntityAux<MapPolygon>
{
    public EntityMultiIndexer<MapPolygon, Peep> PeepsInPoly { get; private set; }
    public IReadOnlyGraph<MapPolygon, PolyBorderChain> BorderGraph { get; private set; }
    public EntityValueCache<MapPolygon, PolyAuxData> AuxDatas { get; private set; }
    public PolyGrid MapPolyGrid { get; private set; }
    public HashSet<MapChunk> Chunks { get; private set; }
    public Dictionary<MapPolygon, MapChunk> ChunksByPoly { get; private set; }
    public LandSeaManager LandSea { get; private set; }
    public RefAction<ValChangeNotice<EntityRef<Regime>>> ChangedRegime { get; private set; }

    public MapPolygonAux(Domain domain, Data data) : base(domain, data)
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
        AuxDatas = EntityValueCache<MapPolygon, PolyAuxData>.ConstructTrigger(
            data,
            p => new PolyAuxData(p, data),
             new RefAction[] {data.Notices.SetPolyShapes, data.Notices.FinishedStateSync},
            new RefAction<Tuple<MapPolygon, PolyAuxData>>[]{}
        );
        ChangedRegime = new RefAction<ValChangeNotice<EntityRef<Regime>>>();
        Game.I.Serializer.GetEntityMeta<MapPolygon>()
            .GetEntityVarMeta<EntityRef<Regime>>(nameof(MapPolygon.Regime))
            .ValChanged().Subscribe(ChangedRegime);
        
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
        
        data.Notices.FinishedStateSync.Subscribe(() =>
        {
            SetupPolyGrid(data);
        });
        data.Notices.SetPolyShapes.Subscribe(() =>
        {
            SetupPolyGrid(data);
        });
    }

    private void SetupPolyGrid(Data data)
    {
        data.Notices.SetPolyShapes.Subscribe(() => BuildPolyGrid(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildPolyGrid(data));
        
        data.Notices.SetPolyShapes.Subscribe(() => BuildChunks(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildChunks(data));
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
        ChunksByPoly = new Dictionary<MapPolygon, MapChunk>();
        Chunks = new HashSet<MapChunk>();
        foreach (var c in regularGrid.Cells)
        {
            var chunk = new MapChunk(c.Value, c.Key);
            Chunks.Add(chunk);
            c.Value.ForEach(p => ChunksByPoly.Add(p, chunk));
        }
    }
}