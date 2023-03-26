using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public EntityRegister<MapPolygon> MapPolygonR => GetRegister<MapPolygon>();
    public EntityRegister<MapPolygonEdge> MapPolygonEdgeR => GetRegister<MapPolygonEdge>();
    public EntityRegister<PlanetInfo> PlanetInfoR => GetRegister<PlanetInfo>();
    public EntityRegister<ResourceDeposit> ResourceDepositR => GetRegister<ResourceDeposit>();

    public MapPolygonRepo Polygons { get; private set; }
    public MapPolygonEdgeRepo PolyEdges { get; private set; }
    public SingletonRepo<PlanetInfo> PlanetInfo { get; private set; }
    public ResourceDepositRepo ResourceDeposits { get; private set; }
    public float Width => PlanetInfo.Value.Dimensions.x;
    public float Height => PlanetInfo.Value.Dimensions.y;
    public PlanetDomain(Data data) : base(data, typeof(BaseDomain))
    {
        Polygons = new MapPolygonRepo(this, data);
        AddRepo(Polygons);

        PolyEdges = new MapPolygonEdgeRepo(this, data);
        AddRepo(PolyEdges);

        PlanetInfo = new SingletonRepo<PlanetInfo>(this, data);
        AddRepo(PlanetInfo);
        
        ResourceDeposits = new ResourceDepositRepo(this, data);
        AddRepo(ResourceDeposits);
    }
    
}