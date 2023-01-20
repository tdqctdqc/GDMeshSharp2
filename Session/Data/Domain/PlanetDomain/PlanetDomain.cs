using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PlanetDomain : Domain
{
    public Repository<GenPolygon> GeoPolygons { get; private set; }
    public Repository<GenCell> Cells { get; private set; }
    public TerrainTriRepo TerrainTris { get; private set; }
    public PlanetDomain(Data data) : base()
    {
        GeoPolygons = new Repository<GenPolygon>(this, data);
        Cells = new Repository<GenCell>(this, data);
        TerrainTris = new TerrainTriRepo(this, data);
        AddRepo(GeoPolygons);
        AddRepo(Cells);
        AddRepo(TerrainTris);
    }
    
}