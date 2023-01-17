using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeoMass : ISuper<GeoMass, GeoPlate>
{
    public int Id { get; private set; }
    public Continent Continent { get; private set; }
    public GeoPlate Seed { get; private set; }
    public GeoPolygon SeedPoly => Seed.SeedPoly;
    public HashSet<GeoPlate> Plates { get; private set; }
    public HashSet<GeoPlate> NeighboringPlates { get; private set; }
    public Dictionary<GeoPlate, int> NeighboringPlatesAdjCount { get; private set; }
    public HashSet<GeoMass> Neighbors { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public Vector2 Center { get; private set; }
    public GeoMass(GeoPlate seed, int id)
    {
        Center = Vector2.Zero;
        Id = id;
        Seed = seed;
        Plates = new HashSet<GeoPlate>();
        NeighboringPlates = new HashSet<GeoPlate>();
        NeighboringPlatesAdjCount = new Dictionary<GeoPlate, int>();
        Neighbors = new HashSet<GeoMass>();
        BoundingBox = new BoundingBox();
        AddPlate(seed);
    }

    public void AddPlate(GeoPlate c)
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
    IReadOnlyCollection<GeoMass> ISuper<GeoMass, GeoPlate>.Neighbors => Neighbors;
    IReadOnlyCollection<GeoPlate> ISuper<GeoMass, GeoPlate>.GetSubNeighbors(GeoPlate plate) => plate.Neighbors;
    GeoMass ISuper<GeoMass, GeoPlate>.GetSubSuper(GeoPlate plate) => plate.Mass;
    IReadOnlyCollection<GeoPlate> ISuper<GeoMass, GeoPlate>.Subs => Plates;
}
