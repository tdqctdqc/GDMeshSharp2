using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public MapPolygonRepository Polygons { get; private set; }
    public MapPolygonBorderRepository PolyBorders { get; private set; }
    public SingletonRepo<PlanetInfo> PlanetInfo { get; private set; }
    public PolyTerrainTriRepo TerrainTris { get; private set; }
    
    //todo fix this to be properly synced
    public float Width => PlanetInfo.Value.Dimensions.x;
    public float Height => PlanetInfo.Value.Dimensions.y;
    public PlanetDomain(Data data) : base(data)
    {
        Polygons = new MapPolygonRepository(this, data);
        AddRepo(Polygons);

        PolyBorders = new MapPolygonBorderRepository(this, data);
        AddRepo(PolyBorders);

        PlanetInfo = new SingletonRepo<PlanetInfo>(this, data);
        AddRepo(PlanetInfo);

        TerrainTris = new PolyTerrainTriRepo(this, data);
        AddRepo(TerrainTris);
    }
    
}