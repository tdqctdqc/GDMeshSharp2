using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class GenCell : ISuper<GenCell, MapPolygon>
{
    public MapPolygon Seed { get; private set; }
    public GenPlate Plate { get; private set; }
    public HashSet<MapPolygon> PolyGeos { get; private set; }
    public HashSet<MapPolygon> NeighboringPolyGeos { get; private set; }
    public List<GenCell> Neighbors { get; private set; }
    public Vector2 Center { get; private set; }
    private Dictionary<MapPolygon, GenCell> _polyCells;
    public GenCell(MapPolygon seed, GenWriteKey key, Dictionary<MapPolygon, GenCell> polyCells, GenData data)
    {
        _polyCells = polyCells;
        Center = Vector2.Zero;
        Seed = seed;
        PolyGeos = new HashSet<MapPolygon>();
        NeighboringPolyGeos = new HashSet<MapPolygon>();
        Neighbors = new List<GenCell>();
        AddPolygon(seed, key);
    }

    public void SetPlate(GenPlate plate, GenWriteKey key)
    {
        Plate = plate;
    }
    public void AddPolygon(MapPolygon p, GenWriteKey key)
    {
        Center = (Center * PolyGeos.Count + p.Center) / (PolyGeos.Count + 1);
        PolyGeos.Add(p);
        _polyCells[p] = this;
        NeighboringPolyGeos.Remove(p);
        var newBorder = p.Neighbors.Refs().Except(PolyGeos);
        foreach (var borderPoly in newBorder)
        {
            NeighboringPolyGeos.Add(borderPoly);
        }
    }


    public void SetNeighbors(GenWriteKey key)
    {
        foreach (var p in NeighboringPolyGeos)
        {
            if (key.GenData.GenAuxData.PolyCells.ContainsKey(p) == false)
            {
                throw new Exception($"No aux data for cell at " + p.Center);
            }
        }
        Neighbors = NeighboringPolyGeos
            .Select(t => key.GenData.GenAuxData.PolyCells[t]).Distinct().ToList();
    }
    IReadOnlyCollection<GenCell> ISuper<GenCell, MapPolygon>.Neighbors => Neighbors;
    IReadOnlyCollection<MapPolygon> ISuper<GenCell, MapPolygon>.GetSubNeighbors(MapPolygon poly) => poly.Neighbors.Refs();
    GenCell ISuper<GenCell, MapPolygon>.GetSubSuper(MapPolygon poly) => _polyCells[poly];
    IReadOnlyCollection<MapPolygon> ISuper<GenCell, MapPolygon>.Subs => PolyGeos;
}
