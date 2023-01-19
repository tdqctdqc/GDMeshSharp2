using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GenCell : ISuper<GenCell, GenPolygon>
{
    public GenPolygon Seed { get; private set; }
    public GenPlate Plate { get; private set; }
    public HashSet<GenPolygon> PolyGeos { get; private set; }
    public HashSet<GenPolygon> NeighboringPolyGeos { get; private set; }
    public List<GenCell> Neighbors { get; private set; }
    public Vector2 Center { get; private set; }
    public GenCell(GenPolygon seed, CreateWriteKey key)
    {
        Center = Vector2.Zero;
        Seed = seed;
        PolyGeos = new HashSet<GenPolygon> {};
        NeighboringPolyGeos = new HashSet<GenPolygon>();
        Neighbors = new List<GenCell>();
        AddPolygon(seed, key);
    }

    public void SetPlate(GenPlate plate)
    {
        if(Plate != null) throw new Exception();
        Plate = plate;
    }
    public void AddPolygon(GenPolygon p, CreateWriteKey key)
    {
        Center = (Center * PolyGeos.Count + p.Center) / (PolyGeos.Count + 1);
        PolyGeos.Add(p);
        p.Set(nameof(GenPolygon.Cell), this, key);
        NeighboringPolyGeos.Remove(p);
        var newBorder = p.GeoNeighbors.Refs.Except(PolyGeos);
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
    
    
    IReadOnlyCollection<GenCell> ISuper<GenCell, GenPolygon>.Neighbors => Neighbors;
    IReadOnlyCollection<GenPolygon> ISuper<GenCell, GenPolygon>.GetSubNeighbors(GenPolygon poly) => poly.GeoNeighbors.Refs;
    GenCell ISuper<GenCell, GenPolygon>.GetSubSuper(GenPolygon poly) => poly.Cell;
    IReadOnlyCollection<GenPolygon> ISuper<GenCell, GenPolygon>.Subs => PolyGeos;
}
