using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeoPlate : ISuper<GeoPlate, GeoCell>
{
    public int Id { get; private set; }
    public GeoCell Seed { get; private set; }
    public GeoPolygon SeedPoly => Seed.Seed;
    public GeoMass Mass { get; private set; }
    public HashSet<GeoCell> Cells { get; private set; }
    public HashSet<GeoCell> NeighboringCells { get; private set; }
    public Dictionary<GeoCell, int> NeighboringCellsAdjCount { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public HashSet<GeoPlate> Neighbors { get; private set; }
    public Vector2 Center => SeedPoly.Center;
    public GeoPlate(GeoCell seed, int id)
    {
        Id = id;
        Seed = seed;
        Cells = new HashSet<GeoCell> {};
        NeighboringCells = new HashSet<GeoCell>();
        NeighboringCellsAdjCount = new Dictionary<GeoCell, int>();
        BoundingBox = new BoundingBox();
        AddCell(seed);
    }

    public void AddCell(GeoCell c)
    {
        Cells.Add(c);
        BoundingBox.Cover(c.BoundingBox);
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

    public void SetContinent(GeoMass c)
    {
        if (Mass != null) throw new Exception();
        Mass = c;
    }
    public IEnumerable<GeoPolygon> GetBorderPolys()
    {
        var borderCellPolys = this
            .GetBorderElements()
            .SelectMany(c => c.PolyGeos);
        var borderPolys = GenerationUtility
            .GetBorderElements(borderCellPolys, p => p.GeoNeighbors, p => p.Cell.Plate != this);
        return borderPolys;
    }
    
    public List<BorderEdge<GeoPolygon>> GetOrderedBorderRelative(GeoPlate aPlate)
    {
        var borderCellPolys = this
            .GetBorderElements()
            .SelectMany(c => c.PolyGeos);
        var borderPolys = GenerationUtility
            .GetBorderElements(borderCellPolys, p => p.GeoNeighbors, n => n.Cell.Plate == aPlate);
        var commonPolyBorders = GenerationUtility.GetOrderedBorderPairs(borderPolys, c => c.GeoNeighbors,
            c => c.Cell.Plate == aPlate);
        
        return commonPolyBorders;
    }
    
    IReadOnlyCollection<GeoPlate> ISuper<GeoPlate, GeoCell>.Neighbors => Neighbors;
    IReadOnlyCollection<GeoCell> ISuper<GeoPlate, GeoCell>.GetSubNeighbors(GeoCell cell) => cell.Neighbors;
    GeoPlate ISuper<GeoPlate, GeoCell>.GetSubSuper(GeoCell cell) => cell.Plate;
    IReadOnlyCollection<GeoCell> ISuper<GeoPlate, GeoCell>.Subs => Cells;
}
