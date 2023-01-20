using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class GenCell : Entity, ISuper<GenCell, GenPolygon>
{
    public GenPolygon Seed { get; private set; }
    public GenPlate Plate { get; private set; }
    public EntityRefCollection<GenPolygon> PolyGeos { get; private set; }
    public EntityRefCollection<GenPolygon> NeighboringPolyGeos { get; private set; }
    public EntityRefCollection<GenCell> Neighbors { get; private set; }
    public Vector2 Center { get; private set; }
    public GenCell(int id, GenPolygon seed, CreateWriteKey key) : base(id, key)
    {
        Center = Vector2.Zero;
        Seed = seed;
        PolyGeos = new EntityRefCollection<GenPolygon>(new int[0], key.Data);
        NeighboringPolyGeos = new EntityRefCollection<GenPolygon>(new int[0], key.Data);
        Neighbors = new EntityRefCollection<GenCell>(new int[0], key.Data);
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
        PolyGeos.Add(p.Id, key.Data);
        var cellRef = new EntityRef<GenCell>(this);
        p.Set(nameof(GenPolygon.Cell), cellRef, key);
        NeighboringPolyGeos.Remove(p.Id, key.Data);
        var newBorder = p.GeoNeighbors.Refs.Except(PolyGeos.Refs);
        foreach (var borderPoly in newBorder)
        {
            NeighboringPolyGeos.Add(borderPoly.Id, key.Data);
        }
    }


    public void SetNeighbors(CreateWriteKey key)
    {
        var newNeighbors = NeighboringPolyGeos.Refs
            .Select(t => t.Cell.Ref).ToHashSet();
        Neighbors.Set(newNeighbors, key.Data);
    }
    
    private static GenCell DeserializeConstructor(string json)
    {
        return new GenCell(json);
    }
    private GenCell(string json) : base(json) { }
    
    
    
    IReadOnlyCollection<GenCell> ISuper<GenCell, GenPolygon>.Neighbors => Neighbors.Refs;
    IReadOnlyCollection<GenPolygon> ISuper<GenCell, GenPolygon>.GetSubNeighbors(GenPolygon poly) => poly.GeoNeighbors.Refs;
    GenCell ISuper<GenCell, GenPolygon>.GetSubSuper(GenPolygon poly) => poly.Cell.Ref;
    IReadOnlyCollection<GenPolygon> ISuper<GenCell, GenPolygon>.Subs => PolyGeos.Refs;
}
