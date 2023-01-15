using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeologyPlate : ISuper<GeologyCell>
{
    public int Id { get; private set; }
    public GeologyCell Seed { get; private set; }
    public GeologyPolygon SeedPoly => Seed.Seed;
    public GeologyMass Mass { get; private set; }
    public HashSet<GeologyCell> Cells { get; private set; }
    public HashSet<GeologyCell> NeighboringCells { get; private set; }
    public Dictionary<GeologyCell, int> NeighboringCellsAdjCount { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public HashSet<GeologyPlate> Neighbors { get; private set; }
    public Vector2 Center => SeedPoly.Center;
    public GeologyPlate(GeologyCell seed, int id)
    {
        Id = id;
        Seed = seed;
        Cells = new HashSet<GeologyCell> {};
        NeighboringCells = new HashSet<GeologyCell>();
        NeighboringCellsAdjCount = new Dictionary<GeologyCell, int>();
        BoundingBox = new BoundingBox();
        AddCell(seed);
    }

    public void AddCell(GeologyCell c)
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

    public void SetContinent(GeologyMass c)
    {
        if (Mass != null) throw new Exception();
        Mass = c;
    }
    public IEnumerable<GeologyPolygon> GetBorderPolys()
    {
        var borderCellPolys = this
            .GetBorderElements()
            .SelectMany(c => c.PolyGeos);
        var borderPolys = GenerationUtility
            .GetBorderElements(borderCellPolys, p => p.GeoNeighbors, p => p.Cell.Plate != this);
        return borderPolys;
    }
    
    public List<BorderEdge<GeologyPolygon>> GetOrderedBorderRelative(GeologyPlate aPlate)
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
    
    IReadOnlyCollection<ISuper<GeologyCell>> ISuper<GeologyCell>.Neighbors => Neighbors;
    IReadOnlyCollection<GeologyCell> ISuper<GeologyCell>.GetSubNeighbors(GeologyCell cell) => cell.Neighbors;
    ISuper<GeologyCell> ISuper<GeologyCell>.GetSubSuper(GeologyCell cell) => cell.Plate;
    IReadOnlyCollection<GeologyCell> ISuper<GeologyCell>.Subs => Cells;
}
