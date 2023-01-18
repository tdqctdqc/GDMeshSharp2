using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Continent : ISuper<Continent, GeoMass>
{
    public int Id { get; private set; }
    public GeoMass Seed { get; private set; }
    public GeoPolygon SeedPoly => Seed.SeedPoly;
    public BoundingBox BoundingBox { get; private set; }
    public HashSet<GeoMass> Masses { get; private set; }
    public HashSet<GeoMass> NeighboringMasses { get; private set; }
    public Dictionary<GeoMass, int> NeighboringMassesAdjCount { get; private set; }
    public HashSet<Continent> Neighbors { get; private set; }
    public Vector2 Drift { get; private set; }
    public Vector2 Center { get; private set; }
    public float Altitude { get; private set; }
    public Continent(GeoMass seed, int id, float altitude)
    {
        Altitude = altitude;
        Center = Vector2.Zero;
        Id = id;
        Seed = seed;
        Masses = new HashSet<GeoMass>();
        NeighboringMasses = new HashSet<GeoMass>();
        NeighboringMassesAdjCount = new Dictionary<GeoMass, int>();
        BoundingBox = new BoundingBox();
        Drift = Vector2.Left.Rotated(Game.I.Random.RandfRange(0f, 2f * Mathf.Pi));
        AddMass(seed);
    }

    public void AddMass(GeoMass c)
    {
        Center = (Center * Masses.Count + c.Center) / (Masses.Count + 1);
        Masses.Add(c);
        BoundingBox.Cover(c.BoundingBox);
        c.SetContinent(this);
        NeighboringMasses.Remove(c);
        NeighboringMassesAdjCount.Remove(c);
        var border = c.Neighbors.Except(Masses);
        foreach (var nPlate in border)
        {
            NeighboringMasses.Add(nPlate);
            if (NeighboringMassesAdjCount.ContainsKey(nPlate) == false)
            {
                NeighboringMassesAdjCount.Add(nPlate, 0);
            }
            NeighboringMassesAdjCount[nPlate]++;
        }
    }
    public void SetNeighbors()
    {
        Neighbors = NeighboringMasses.Select(t => t.Continent).ToHashSet();
    }
    
    IReadOnlyCollection<Continent> ISuper<Continent, GeoMass>.Neighbors => Neighbors;
    IReadOnlyCollection<GeoMass> ISuper<Continent, GeoMass>.GetSubNeighbors(GeoMass mass) => mass.Neighbors;
    Continent ISuper<Continent, GeoMass>.GetSubSuper(GeoMass mass) => mass.Continent;
    IReadOnlyCollection<GeoMass> ISuper<Continent, GeoMass>.Subs => Masses;
}
