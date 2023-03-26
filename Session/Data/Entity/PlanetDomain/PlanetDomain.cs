using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public EntityRegister<MapPolygon> Polygons => GetRegister<MapPolygon>();
    public EntityRegister<MapPolygonEdge> PolyEdges => GetRegister<MapPolygonEdge>();
    public EntityRegister<ResourceDeposit> ResourceDeposits => GetRegister<ResourceDeposit>();
    public MapPolygonRepo PolygonAux { get; private set; }
    public MapPolygonEdgeRepo PolyEdgeAux { get; private set; }
    public PlanetInfo Info => _planetInfoAux.Value;
    private SingletonAux<PlanetInfo> _planetInfoAux;
    public ResourceDepositRepo ResourceDepositAux { get; private set; }
    public float Width => _planetInfoAux.Value.Dimensions.x;
    public float Height => _planetInfoAux.Value.Dimensions.y;
    public PlanetDomain() : base(typeof(PlanetDomain))
    {
        
    }

    protected override void Setup()
    {
        PolygonAux = new MapPolygonRepo(this, Data);
        PolyEdgeAux = new MapPolygonEdgeRepo(this, Data);
        _planetInfoAux = new SingletonAux<PlanetInfo>(this, Data);
        ResourceDepositAux = new ResourceDepositRepo(this, Data);
        AddRepo(PolygonAux);
        AddRepo(PolyEdgeAux);
        AddRepo(_planetInfoAux);
        AddRepo(ResourceDepositAux);
    }
}