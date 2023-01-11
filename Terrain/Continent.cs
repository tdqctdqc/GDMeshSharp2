using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Continent : ISuper<GeologyMass>
{
    public int Id { get; private set; }
    public GeologyMass Seed { get; private set; }
    public BoundingBox BoundingBox { get; private set; }
    public HashSet<GeologyMass> Masses { get; private set; }
    public HashSet<GeologyMass> NeighboringMasses { get; private set; }
    public Dictionary<GeologyMass, int> NeighboringMassesAdjCount { get; private set; }
    public HashSet<Continent> Neighbors { get; private set; }
    public Vector2 Drift { get; private set; }
    public Vector2 Center { get; private set; }
    public float Altitude { get; private set; }
    public Continent(GeologyMass seed, int id, float altitude)
    {
        Altitude = altitude;
        Center = Vector2.Zero;
        Id = id;
        Seed = seed;
        Masses = new HashSet<GeologyMass>();
        NeighboringMasses = new HashSet<GeologyMass>();
        NeighboringMassesAdjCount = new Dictionary<GeologyMass, int>();
        BoundingBox = new BoundingBox();
        Drift = Vector2.Left.Rotated(Root.Random.RandfRange(0f, 2f * Mathf.Pi));
        AddMass(seed);
    }

    public void AddMass(GeologyMass c)
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
    
    IReadOnlyCollection<ISuper<GeologyMass>> ISuper<GeologyMass>.Neighbors => Neighbors;
    IReadOnlyCollection<GeologyMass> ISuper<GeologyMass>.GetSubNeighbors(GeologyMass mass) => mass.Neighbors;
    ISuper<GeologyMass> ISuper<GeologyMass>.GetSubSuper(GeologyMass mass) => mass.Continent;
    IReadOnlyCollection<GeologyMass> ISuper<GeologyMass>.Subs => Masses;
}
