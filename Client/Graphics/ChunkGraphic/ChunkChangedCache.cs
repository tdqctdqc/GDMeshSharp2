using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkChangedCache
{
    public ChunkChangeListener BuildingsChanged { get; private set; }
    public ChunkChangeListener RoadsChanged { get; private set; }
    public ChunkChangeListener TerrainChanged { get; private set; }
    public ChunkChangeListener PolyRegimeChanged { get; private set; }
    public void Clear()
    {
        BuildingsChanged?.Clear();
        RoadsChanged?.Clear();
        TerrainChanged?.Clear();
        PolyRegimeChanged?.Clear();
    }
    public ChunkChangedCache(Data d)
    {
        BuildingsChanged = ChunkChangeListener.ConstructConstant<MapBuilding>(d,
            mb => mb.Position.Poly(d));
        RoadsChanged = ChunkChangeListener.ConstructConstantMultiple<RoadSegment>(d,
            mb =>
            {
                var e = mb.Edge.Entity();
                return new[] {e.HighPoly.Entity(), e.LowPoly.Entity()};
            });
        var px = d.Planet.PolygonAux;
        var cr = d.Planet.PolygonAux.ChangedRegime;
        PolyRegimeChanged = ChunkChangeListener.ConstructDynamic<MapPolygon, EntityRef<Regime>>
            (d, cr, mp => mp);
    }
}
