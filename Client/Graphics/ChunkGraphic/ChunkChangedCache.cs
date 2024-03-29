using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkChangedCache
{
    public ChunkChangeListener<int> BuildingsChanged { get; private set; }
    public ChunkChangeListener<int> RoadsChanged { get; private set; }
    // public ChunkChangeListener TerrainChanged { get; private set; }
    public ChunkChangeListener<int> PolyRegimeChanged { get; private set; }
    public ChunkChangeListener<Construction> ConstructionsChanged { get; private set; }
    public ChunkChangeListener<int> SettlementTierChanged { get; private set; }
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
        BuildingsChanged = new ChunkChangeListener<int>(d);
        BuildingsChanged.ListenForEntityCreationDestruction<MapBuilding, int>(d,
            mb => mb.Id, mb => mb.Position.Poly(d));

        RoadsChanged = new ChunkChangeListener<int>(d);
        RoadsChanged.ListenForEntityCreationDestructionMult<RoadSegment, int>(
            d,
            e => e.Edge.Entity().Id,
            e => new MapPolygon[]{e.Edge.Entity().HighPoly.Entity(), e.Edge.Entity().LowPoly.Entity()});
        
        var px = d.Planet.PolygonAux;
        var changedRegime = d.Planet.PolygonAux.ChangedRegime;
        
        PolyRegimeChanged = new ChunkChangeListener<int>(d);
        PolyRegimeChanged.ListenForValueChange<int, MapPolygon, EntityRef<Regime>>(
            d, changedRegime, p => p, p => p.Id);

        ConstructionsChanged = new ChunkChangeListener<Construction>(d);
        ConstructionsChanged.ListenDynamic<Construction, Construction>(
            d, 
            v => v.Pos.Poly(d),
            v => v,
            d.Notices.StartedConstruction,
            d.Notices.EndedConstruction
        );
        
        SettlementTierChanged = new ChunkChangeListener<int>(d);
        SettlementTierChanged.ListenForValueChange<int, Settlement, ModelRef<SettlementTier>>(
            d,
            d.Society.SettlementAux.ChangedTier,
            e => e.Poly.Entity(),
            s => s.Poly.RefId
        );
    }
}
