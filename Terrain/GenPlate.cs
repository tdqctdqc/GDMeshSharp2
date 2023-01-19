using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GenPlate : ISuper<GenPlate, GenCell>
{
    public int Id { get; private set; }
    public GenCell Seed { get; private set; }
    public GenPolygon GetSeedPoly() => Seed.Seed;
    public GenMass Mass { get; private set; }
    public HashSet<GenCell> Cells { get; private set; }
    public HashSet<GenCell> NeighboringCells { get; private set; }
    public Dictionary<GenCell, int> NeighboringCellsAdjCount { get; private set; }
    public HashSet<GenPlate> Neighbors { get; private set; }
    public Vector2 Center => GetSeedPoly().Center;
    public GenPlate(GenCell seed, int id)
    {
        Id = id;
        Seed = seed;
        Cells = new HashSet<GenCell> {};
        NeighboringCells = new HashSet<GenCell>();
        NeighboringCellsAdjCount = new Dictionary<GenCell, int>();
        AddCell(seed);
    }

    public void AddCell(GenCell c)
    {
        Cells.Add(c);
        c.SetPlate(this);
        NeighboringCells.Remove(c);
        var border = c.Neighbors.Except(Cells);
        foreach (var cell in border)
        {
            NeighboringCells.Add(cell);
            if (NeighboringCellsAdjCount.ContainsKey(cell) == false)
            {
                NeighboringCellsAdjCount.Add(cell, 0);
            }
            NeighboringCellsAdjCount[cell]++;
        }
    }

    public void SetNeighbors()
    {
        Neighbors = NeighboringCells.Select(t => t.Plate).ToHashSet();
    }

    public void SetContinent(GenMass c)
    {
        if (Mass != null) throw new Exception();
        Mass = c;
    }
    public IEnumerable<GenPolygon> GetBorderPolys()
    {
        var borderCellPolys = this
            .GetBorderElements()
            .SelectMany(c => c.PolyGeos);
        var borderPolys = GenerationUtility
            .GetBorderElements(borderCellPolys, p => p.GeoNeighbors.Refs, p => p.Cell.Plate != this);
        return borderPolys;
    }
    
    public List<BorderEdge<GenPolygon>> GetOrderedBorderRelative(GenPlate aPlate)
    {
        var borderCellPolys = this
            .GetBorderElements()
            .SelectMany(c => c.PolyGeos);
        var borderPolys = GenerationUtility
            .GetBorderElements(borderCellPolys, p => p.GeoNeighbors.Refs, n => n.Cell.Plate == aPlate);
        var commonPolyBorders = GenerationUtility.GetOrderedBorderPairs(borderPolys, c => c.GeoNeighbors.Refs,
            c => c.Cell.Plate == aPlate);
        
        return commonPolyBorders;
    }
    
    IReadOnlyCollection<GenPlate> ISuper<GenPlate, GenCell>.Neighbors => Neighbors;
    IReadOnlyCollection<GenCell> ISuper<GenPlate, GenCell>.GetSubNeighbors(GenCell cell) => cell.Neighbors;
    GenPlate ISuper<GenPlate, GenCell>.GetSubSuper(GenCell cell) => cell.Plate;
    IReadOnlyCollection<GenCell> ISuper<GenPlate, GenCell>.Subs => Cells;
}
