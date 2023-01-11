using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeologyMass : ISuper<GeologyPlate>
{
    public int Id { get; private set; }
    public Continent Continent { get; private set; }
    public GeologyPlate Seed { get; private set; }
    public HashSet<GeologyPlate> Plates { get; private set; }
    public HashSet<GeologyPlate> NeighboringPlates { get; private set; }
    public Dictionary<GeologyPlate, int> NeighboringPlatesAdjCount { get; private set; }
    public HashSet<GeologyMass> Neighbors { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public Vector2 Center { get; private set; }
    public GeologyMass(GeologyPlate seed, int id)
    {
        Center = Vector2.Zero;
        Id = id;
        Seed = seed;
        Plates = new HashSet<GeologyPlate>();
        NeighboringPlates = new HashSet<GeologyPlate>();
        NeighboringPlatesAdjCount = new Dictionary<GeologyPlate, int>();
        Neighbors = new HashSet<GeologyMass>();
        BoundingBox = new BoundingBox();
        AddPlate(seed);
    }

    public void AddPlate(GeologyPlate c)
    {
        Center = (Center * Plates.Count + c.Center) / (Plates.Count + 1);
        Plates.Add(c);
        BoundingBox.Cover(c.BoundingBox);
        c.SetContinent(this);
        NeighboringPlates.Remove(c);
        NeighboringPlatesAdjCount.Remove(c);
        var border = c.Neighbors.Except(Plates);
        foreach (var nPlate in border)
        {
            NeighboringPlates.Add(nPlate);
            if (NeighboringPlatesAdjCount.ContainsKey(nPlate) == false)
            {
                NeighboringPlatesAdjCount.Add(nPlate, 0);
            }
            NeighboringPlatesAdjCount[nPlate]++;
        }
    }

    public void SetContinent(Continent c)
    {
        Continent = c;
    }
    public void SetNeighbors()
    {
        Neighbors = NeighboringPlates.Select(t => t.Mass).ToHashSet();
    }
    IReadOnlyCollection<ISuper<GeologyPlate>> ISuper<GeologyPlate>.Neighbors => Neighbors;
    IReadOnlyCollection<GeologyPlate> ISuper<GeologyPlate>.GetSubNeighbors(GeologyPlate plate) => plate.Neighbors;
    ISuper<GeologyPlate> ISuper<GeologyPlate>.GetSubSuper(GeologyPlate plate) => plate.Mass;
    IReadOnlyCollection<GeologyPlate> ISuper<GeologyPlate>.Subs => Plates;
}
