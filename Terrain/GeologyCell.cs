using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeologyCell : ISuper<GeologyPolygon>
{
    public GeologyPolygon Seed { get; private set; }
    public GeologyPlate Plate { get; private set; }
    public HashSet<GeologyPolygon> PolyGeos { get; private set; }
    public HashSet<GeologyPolygon> NeighboringPolyGeos { get; private set; }
    public List<GeologyCell> Neighbors { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public Vector2 Center { get; private set; }
    public GeologyCell(GeologyPolygon seed)
    {
        Center = Vector2.Zero;
        Seed = seed;
        PolyGeos = new HashSet<GeologyPolygon> {};
        NeighboringPolyGeos = new HashSet<GeologyPolygon>();
        Neighbors = new List<GeologyCell>();
        BoundingBox = new BoundingBox();
        AddPolygon(seed);
        
    }

    public void SetPlate(GeologyPlate plate)
    {
        if(Plate != null) throw new Exception();
        Plate = plate;
    }
    public void AddPolygon(GeologyPolygon p)
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
    
    
    IReadOnlyCollection<ISuper<GeologyPolygon>> ISuper<GeologyPolygon>.Neighbors => Neighbors;
    IReadOnlyCollection<GeologyPolygon> ISuper<GeologyPolygon>.GetSubNeighbors(GeologyPolygon poly) => poly.GeoNeighbors;
    ISuper<GeologyPolygon> ISuper<GeologyPolygon>.GetSubSuper(GeologyPolygon poly) => poly.Cell;
    IReadOnlyCollection<GeologyPolygon> ISuper<GeologyPolygon>.Subs => PolyGeos;
}
