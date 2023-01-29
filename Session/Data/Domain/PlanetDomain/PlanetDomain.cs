using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public MapPolygonRepository Polygons { get; private set; }
    public MapPolygonBorderRepository PolyBorders { get; private set; }
    public TerrainTriRepo TerrainTris { get; private set; }
    public SingletonRepo<PlanetInfo> PlanetInfo { get; private set; }
    
    //todo fix this to be properly synced
    public float Width => PlanetInfo.Value.Dimensions.x;
    public float Height => PlanetInfo.Value.Dimensions.y;
    public PlanetDomain(Data data) : base()
    {
        Polygons = new MapPolygonRepository(this, data);
        TerrainTris = new TerrainTriRepo(this, data);
        PolyBorders = new MapPolygonBorderRepository(this, data);
        PlanetInfo = new SingletonRepo<PlanetInfo>(this, data);
        AddRepo(Polygons);
        AddRepo(TerrainTris);
        AddRepo(PolyBorders);
        AddRepo(PlanetInfo);
    }
    
}