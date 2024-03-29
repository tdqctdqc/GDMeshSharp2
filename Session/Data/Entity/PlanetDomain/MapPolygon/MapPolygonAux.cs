using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class MapPolygonAux : EntityAux<MapPolygon>
{
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
        AuxDatas = EntityValueCache<MapPolygon, PolyAuxData>.ConstructConstant(
            data,
            p => new PolyAuxData(p, data)
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
        
        data.Notices.SetPolyShapes.Subscribe(() => BuildPolyGrid(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildPolyGrid(data));
        
        data.Notices.SetPolyShapes.Subscribe(() => BuildChunks(data));
        data.Notices.FinishedStateSync.Subscribe(() => BuildChunks(data));
        
        data.Notices.SetPolyShapes.Subscribe(() => UpdateAuxDatas(data));
    }

    private void UpdateAuxDatas(Data data)
    {
        foreach (var kvp in AuxDatas.Dic)
        {
            var aux = kvp.Value;
            if (aux.Stale)
            {
                var poly = kvp.Key;
                aux.Update(poly, data);
                aux.MarkFresh();
            }
        }
    }
    private void BuildPolyGrid(Data data)
    {
        var sw = new Stopwatch();
        var gridCellSize = 1000f;
        var numPartitions = Mathf.CeilToInt(data.Planet.Info.Dimensions.x / gridCellSize);
        MapPolyGrid = new PolyGrid(numPartitions, data.Planet.Info.Dimensions, data);
        foreach (var p in data.Planet.Polygons.Entities)
        {
            if(p.NeighborBorders.Count > 0) MapPolyGrid.AddElement(p);
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