using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepGenerator : Generator
{
    public GenData Data { get; private set; }
    private IdDispenser _id;
    private GenWriteKey _key;
    public PeepGenerator()
    {
        
    }

    public override GenReport Generate(GenWriteKey key)
    {
        _key = key;
        _id = key.IdDispenser;
        Data = key.GenData;
        var report = new GenReport(GetType().Name);
        
        report.StartSection();
        var farmers = GeneratePeepType
        (
            PeepJobManager.Farmer, 
            p =>
            {
                var sample = new MapPolyTerrainSample(p, Data);
                var fertilityAreaPerFarmerSize = 10f;
                return Mathf.FloorToInt(sample.FertilityMod * p.GetArea(Data) / fertilityAreaPerFarmerSize);
            },
            50, 
            100
        );
        report.StopSection("Farmers");
        
        report.StartSection();
        var byPoly = Data.Society.Settlements.ByPoly;
        var laborers = GeneratePeepType(PeepJobManager.Laborer,
            p => (int)(byPoly.ContainsKey(p)
                ? byPoly[p].Size * 100
                : 0), 
            50, 50);
        report.StopSection("Laborers");

        return report;
    }
    
    private Vector2 GeneratePeepType(PeepJob job, Func<MapPolygon, int> getPoints,
        int minPoints, int pointsPerPeep)
    {
        var numPeeps = 0;
        var totalPoints = 0;
        foreach (var poly in Data.Planet.Polygons.Entities)
        {
            var pointsInPoly = getPoints(poly);
            totalPoints += pointsInPoly;
            if (pointsInPoly < minPoints)
            {
                continue;
            }
            
            var polyPeeps = Mathf.CeilToInt(pointsInPoly / pointsPerPeep);

            numPeeps += polyPeeps;
            for (int i = 0; i < polyPeeps; i++)
            {
                var peepSize = pointsInPoly / (polyPeeps + 1);
                totalPoints += peepSize;
                var peep = Peep.Create(
                    peepSize,
                    new EntityRef<MapPolygon>(poly, _key),
                    new ModelRef<PeepJob>(job, _key),
                    _key);
            }
        }

        return new Vector2(numPeeps, totalPoints);
    }
}