using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeoCell : ISuper<GeoCell, GeoPolygon>
{
    public GeoPolygon Seed { get; private set; }
    public GeoPlate Plate { get; private set; }
    public HashSet<GeoPolygon> PolyGeos { get; private set; }
    public HashSet<GeoPolygon> NeighboringPolyGeos { get; private set; }
    public List<GeoCell> Neighbors { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public Vector2 Center { get; private set; }
    public GeoCell(GeoPolygon seed)
    {
        Center = Vector2.Zero;
        Seed = seed;
        PolyGeos = new HashSet<GeoPolygon> {};
        NeighboringPolyGeos = new HashSet<GeoPolygon>();
        Neighbors = new List<GeoCell>();
        BoundingBox = new BoundingBox();
        AddPolygon(seed);
        
    }

    public void SetPlate(GeoPlate plate)
    {
        if(Plate != null) throw new Exception();
        Plate = plate;
    }
    public void AddPolygon(GeoPolygon p)
    {
        Center = (Center * PolyGeos.Count + p.Center) / (PolyGeos.Count + 1);
        PolyGeos.Add(p);
        p.SetCell(this);
        NeighboringPolyGeos.Remove(p);
        var newBorder = p.GeoNeighbors.Except(PolyGeos);
        BoundingBox.Cover(p.BoundingBox);
        foreach (var borderPoly in newBorder)
        {
            NeighboringPolyGeos.Add(borderPoly);
        }
    }


    public void SetNeighbors()
    {
        Neighbors = NeighboringPolyGeos
            .Select(t => t.Cell)
            .ToList();
    }
    
    
    IReadOnlyCollection<GeoCell> ISuper<GeoCell, GeoPolygon>.Neighbors => Neighbors;
    IReadOnlyCollection<GeoPolygon> ISuper<GeoCell, GeoPolygon>.GetSubNeighbors(GeoPolygon poly) => poly.GeoNeighbors;
    GeoCell ISuper<GeoCell, GeoPolygon>.GetSubSuper(GeoPolygon poly) => poly.Cell;
    IReadOnlyCollection<GeoPolygon> ISuper<GeoCell, GeoPolygon>.Subs => PolyGeos;
}
