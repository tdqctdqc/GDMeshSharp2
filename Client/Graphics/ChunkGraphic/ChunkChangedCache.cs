using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkChangedCache
{
    public ChunkChangeListener BuildingsChanged { get; private set; }
    public ChunkChangeListener RoadsChanged { get; private set; }
    // public ChunkChangeListener TerrainChanged { get; private set; }
    public ChunkChangeListener PolyRegimeChanged { get; private set; }
    public ChunkChangeListener ConstructionsChanged { get; private set; }
    public ChunkChangeListener SettlementTierChanged { get; private set; }
    public void Clear()
    {
        BuildingsChanged.Clear();
        RoadsChanged.Clear();
        // TerrainChanged?.Clear();
        PolyRegimeChanged.Clear();
        ConstructionsChanged.Clear();
    }
    public ChunkChangedCache(Data d)
    {
        BuildingsChanged = ChunkChangeListener.ListenForEntityCreateDestroy<MapBuilding>(d,
            mb => mb.Position.Poly(d));
        RoadsChanged = ChunkChangeListener.ListenForEntityCreateDestroyMult<RoadSegment>(d,
            mb =>
            {
                var e = mb.Edge.Entity();
                return new[] {e.HighPoly.Entity(), e.LowPoly.Entity()};
            });
        var px = d.Planet.PolygonAux;
        var changedRegime = d.Planet.PolygonAux.ChangedRegime;
        PolyRegimeChanged = ChunkChangeListener.ListenForValueChange<MapPolygon, EntityRef<Regime>>
            (d, changedRegime, mp => mp);
        ConstructionsChanged = ChunkChangeListener.ListenDynamic(d, 
            d.Notices.StartedConstruction,
            d.Notices.EndedConstruction);
        SettlementTierChanged = ChunkChangeListener.ListenForValueChange<Settlement, ModelRef<SettlementTier>>
            (d, d.Society.SettlementAux.ChangedTier, s => s.Poly.Entity());
    }
}
