using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepGenerator
{
    public GenData Data { get; private set; }
    private IdDispenser _id;
    private GenWriteKey _key;
    public PeepGenerator(IdDispenser id, GenWriteKey key, GenData data)
    {
        _id = id;
        _key = key;
        Data = data;
    }

    public void Generate()
    {
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
        var byPoly = Data.Society.Settlements.ByPoly;
        var laborers = GeneratePeepType(PeepJobManager.Laborer,
            p => (int)(byPoly.ContainsKey(p.Id)
                ? byPoly[p.Id].Size * 100
                : 0), 
            50, 50);
        
        GD.Print("num peeps " + (farmers.x + laborers.x));
        GD.Print("num farmers " + farmers.x);
        GD.Print("num laborers " + laborers.x);
        // GD.Print("total pop " + _totalPop);
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
            // poly.Log(100, pointsInPoly.ToString());

            numPeeps += polyPeeps;
            for (int i = 0; i < polyPeeps; i++)
            {
                var peepSize = pointsInPoly / (polyPeeps + 1);
                totalPoints += peepSize;
                var peep = Peep.Create(_id.GetID(),
                    peepSize,
                    new EntityRef<MapPolygon>(poly, _key),
                    new ModelRef<PeepJob>(job, _key),
                    _key);
            }
        }

        return new Vector2(numPeeps, totalPoints);
    }
}