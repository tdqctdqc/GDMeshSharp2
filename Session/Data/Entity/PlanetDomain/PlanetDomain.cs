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
    public PolyEdgeAux PolyEdgeAux { get; private set; }
    public PlanetInfo Info => _planetInfoAux != null ? _planetInfoAux.Value : null;
    private SingletonAux<PlanetInfo> _planetInfoAux;
    public ResourceDepositAux ResourceDepositAux { get; private set; }
    public float Width => _planetInfoAux.Value.Dimensions.x;
    public float Height => _planetInfoAux.Value.Dimensions.y;
    public PlanetDomain() : base(typeof(PlanetDomain))
    {
        
    }
    protected override void Setup()
    {
        _planetInfoAux = new SingletonAux<PlanetInfo>(this, Data);
        PolygonAux = new MapPolygonRepo(this, Data);
        PolyEdgeAux = new PolyEdgeAux(this, Data);
        ResourceDepositAux = new ResourceDepositAux(this, Data);
    }
}