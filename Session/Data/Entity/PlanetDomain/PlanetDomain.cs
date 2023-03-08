using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public MapPolygonRepo Polygons { get; private set; }
    public MapPolygonEdgeRepo PolyEdges { get; private set; }
    public SingletonRepo<PlanetInfo> PlanetInfo { get; private set; }
    public PolyTerrainTriRepo TerrainTris { get; private set; }
    public float Width => PlanetInfo.Value.Dimensions.x;
    public float Height => PlanetInfo.Value.Dimensions.y;
    public PlanetDomain(Data data) : base(data)
    {
        Polygons = new MapPolygonRepo(this, data);
        AddRepo(Polygons);

        PolyEdges = new MapPolygonEdgeRepo(this, data);
        AddRepo(PolyEdges);

        PlanetInfo = new SingletonRepo<PlanetInfo>(this, data);
        AddRepo(PlanetInfo);

        TerrainTris = new PolyTerrainTriRepo(this, data);
        AddRepo(TerrainTris);
    }
    
}