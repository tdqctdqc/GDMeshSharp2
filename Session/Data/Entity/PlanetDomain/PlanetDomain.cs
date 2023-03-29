using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public EntityRegister<MapPolygon> Polygons => GetRegister<MapPolygon>();
    public EntityRegister<MapPolygonEdge> PolyEdges => GetRegister<MapPolygonEdge>();
    public EntityRegister<ResourceDeposit> ResourceDeposits => GetRegister<ResourceDeposit>();
    public MapPolygonAux PolygonAux { get; private set; }
    public PolyEdgeAux PolyEdgeAux { get; private set; }
    public PlanetInfo Info => _planetInfoAux != null ? _planetInfoAux.Value : null;
    private SingletonAux<PlanetInfo> _planetInfoAux;
    public ResourceDepositAux ResourceDepositAux { get; private set; }
    public float Width => _planetInfoAux.Value.Dimensions.x;
    public float Height => _planetInfoAux.Value.Dimensions.y;
    public PlanetDomain(Data data) : base(typeof(PlanetDomain), data)
    {
        
    }
    public override void Setup()
    {
        _planetInfoAux = new SingletonAux<PlanetInfo>(this, Data);
        PolygonAux = new MapPolygonAux(this, Data);
        PolyEdgeAux = new PolyEdgeAux(this, Data);
        ResourceDepositAux = new ResourceDepositAux(this, Data);
    }
}