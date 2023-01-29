using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorNetStd;

public class GeologyGenerator
{
    public GenData Data { get; private set; }
    private IDDispenser _id;
    private GenWriteKey _key;
    public GeologyGenerator(GenData data, IDDispenser id)
    {
        _id = id;
        Data = data;
    }
    public void GenerateTerrain(GenWriteKey key)
    {
        _key = key;
        EdgeDisturber.DisturbEdges(Data.Planet.Polygons.Entities, Data.Planet.PlanetInfo.Value.Dimensions, key);
        BuildGeology();
    }

    private void BuildGeology()
    {
        BuildCells();
        BuildPlates();
        BuildMasses();
        BuildContinents();
        DoContinentFriction();
        HandleIsthmusAndInlandSeas();
        BuildLandformTris();
        Data.LandSea.SetLandmasses(Data);
    }

    private void HandleIsthmusAndInlandSeas()
    {
        
    }

    
    private void BuildCells()
    {
        var polysPerCell = 3;
        var numCells = Data.Planet.Polygons.Entities.Count / polysPerCell;
        var polyCellDic = Data.GenAuxData.PolyCells;
        var cellSeeds = GenerationUtility.PickSeeds(Data.Planet.Polygons.Entities, new int[] {numCells})[0];
        var cells = cellSeeds.Select(p => new GenCell(p, _key, polyCellDic, Data)).ToList();
        Data.GenAuxData.Cells.AddRange(cells);
        GD.Print("Num cells: " + cells.Count);
        var polysNotTaken =
            Data.Planet.Polygons.Entities.Except(cellSeeds);
        var remainder = GenerationUtility.PickInTurn(polysNotTaken, cells, 
            cell => cell.NeighboringPolyGeos, 
            (cell, poly) => cell.AddPolygon(poly, _key));

        if (remainder.Count > 0) throw new Exception();
        cells.ForEach(c => c.SetNeighbors(_key));
        
    }

    private void BuildPlates()
    {
        var cellsPerPlate = 3;
        var numPlates = Data.GenAuxData.Cells.Count / cellsPerPlate;
        var plateSeeds = GenerationUtility.PickSeeds(Data.GenAuxData.Cells, new[] {numPlates})[0];
        var plates = plateSeeds.Select(s => new GenPlate(s, _id.GetID(), _key)).ToList();
        GD.Print("Num plates: " + plates.Count);
        
        Data.GenAuxData.Plates.AddRange(plates);
        var cellsNotTaken = Data.GenAuxData.Cells.Except(plateSeeds);
        var remainder = GenerationUtility.PickInTurnHeuristic(cellsNotTaken, plates, 
            plate => plate.NeighboringCells,
            (plate, cell) => plate.AddCell(cell, _key),
            ((cell, plate) => plate.NeighboringCellsAdjCount[cell]));
        if (remainder.Count > 0) throw new Exception();
        plates.ForEach(p =>
        {
            p.SetNeighbors();
        });
        foreach (var poly in Data.Planet.Polygons.Entities)
        {
            var cell = Data.GenAuxData.PolyCells[poly];
            var plate = cell.Plate;
        }
    }

    private void BuildMasses()
    {
        var platesPerMass = 3;
        var numMasses = Data.GenAuxData.Plates.Count / 3;
        var massSeeds = GenerationUtility.PickSeeds(Data.GenAuxData.Plates, new int[] {numMasses})[0];
        var masses = massSeeds.Select(s => new GenMass(s, _id.GetID())).ToList();
        GD.Print("Num masses: " + masses.Count);

        var platesNotTaken = Data.GenAuxData.Plates.Except(massSeeds);
        var remainder = GenerationUtility.PickInTurnHeuristic(platesNotTaken, masses,
            mass => mass.NeighboringPlates,
            (mass, plate) => mass.AddPlate(plate),
            (plate, mass) => mass.NeighboringPlatesAdjCount[plate]);
        if (remainder.Count > 0) throw new Exception();
        
        Data.GenAuxData.Masses.AddRange(masses);
        masses.ForEach(m => m.SetNeighbors());
    }

    private void BuildContinents()
    {
        var massesPerCont = 3;
        var numMasses = Data.GenAuxData.Masses.Count / 3;
        var numLandmasses = numMasses / 4;
        var numSeas = numMasses * 3 / 4;
        if (numLandmasses + numSeas > Data.GenAuxData.Masses.Count) throw new Exception();
        var seeds = GenerationUtility.PickSeeds(Data.GenAuxData.Masses, new int[] {numLandmasses, numSeas});
        var landSeeds = seeds[0].ToHashSet();
        var waterSeeds = seeds[1].ToHashSet();
        var allSeeds = landSeeds.Union(waterSeeds);
        var conts = landSeeds.Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(.6f, .9f)))
            .Union(waterSeeds.Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(.1f, .45f))))
            .ToList();
        GD.Print("Num conts: " + conts.Count);

        var remainder = GenerationUtility.PickInTurn(Data.GenAuxData.Masses.Except(allSeeds), conts,
            cont => cont.NeighboringMasses,
            (cont, mass) => cont.AddMass(mass));

        if (remainder.Count > 0) throw new Exception();
        
        
        Data.GenAuxData.Continents.AddRange(conts);
        conts.ForEach(c => c.SetNeighbors());
        Data.GenAuxData.Continents.ForEach(cont =>
        {
            var isLand = landSeeds.Contains(cont.Seed);
            var polys = cont.Masses
                .SelectMany(m => m.Plates)
                .SelectMany(p => p.Cells)
                .SelectMany(c => c.PolyGeos);
            foreach (var poly in polys)
            {
                var altValue = Game.I.Random.RandfRange(.8f * cont.Altitude, 1.2f * cont.Altitude);
                poly.Set(nameof(MapPolygon.Altitude), altValue, _key);
                //todo make this sample perlin
            }
        });
    }

    private void DoContinentFriction()
    {
        Data.GenAuxData.Plates.ForEach(p => setFriction(p));
        Data.GenAuxData.FaultLines.ForEach(f =>
        {
            var inRange = getPolysInRangeOfFault(f);
            f.PolyFootprint.AddRange(inRange);   
        });
        


        //find polys in range of fault
        //add altitude + roughness to them
        //union find for the ones that are land and eligible for hill, mtn, etc
        //draw meshes inside these 
        
        
        
        void setFriction(GenPlate hiPlate)
        {
            var neighbors = hiPlate.Neighbors.ToList();
            var count = neighbors.Count;
            for (var j = 0; j < count; j++)
            {
                var loPlate = neighbors[j];
                if (loPlate.Id < hiPlate.Id 
                    && loPlate.Mass.GenContinent != hiPlate.Mass.GenContinent)
                {
                    var drift1 = hiPlate.Mass.GenContinent.Drift;
                    var drift2 = loPlate.Mass.GenContinent.Drift;

                    var axis = loPlate.Center - hiPlate.Center;
                    var driftStr = (drift1 - drift2).Length() / 2f;
                    if (driftStr > .75f)
                    {
                        var borders = hiPlate.GetOrderedBorderRelative(loPlate, Data);
                        var fault = new FaultLine(driftStr, hiPlate, loPlate, borders, Data);
                        Data.GenAuxData.FaultLines.Add(fault);
                    }
                }
            }
        }

        IEnumerable<MapPolygon> getPolysInRangeOfFault(FaultLine fault)
        {
            var faultRange = fault.Friction * 500f;
            var polys = fault.HighId.Cells.SelectMany(c => c.PolyGeos)
                .Union(fault.LowId.Cells.SelectMany(c => c.PolyGeos));
            var frictionAltEffect = .1f;
            var frictionRoughnessEffect = 1f;
            var polysInRange = new List<MapPolygon>();
            foreach (var poly in polys)
            {
                var dist = fault.GetDist(poly, Data);
                var distRatio = (faultRange - dist) / faultRange;
                if (dist < faultRange)
                {
                    polysInRange.Add(poly);
                    var altIncrement = fault.Friction * frictionAltEffect * distRatio;
                    float erosion = 0f;
                    if (poly.Altitude < .5f) erosion = poly.Altitude;
                    poly.Set(nameof(poly.Altitude), poly.Altitude + altIncrement, _key);
                    
                    
                    
                    var rand = Game.I.Random.RandfRange(-.2f, .2f);
                    var newRoughness = Mathf.Clamp(fault.Friction * frictionRoughnessEffect * distRatio - erosion + rand, 0f,
                        1f);
                    poly.Set(nameof(poly.Roughness), newRoughness, _key);
                }
            }

            return polysInRange;
        }
    }
    private void BuildLandformTris()
    {   
        Data.Models.Landforms.BuildTriHolders(_id, Data, _key);
        var affectedPolys = Data.GenAuxData.FaultLines.SelectMany(f => f.PolyFootprint).Where(p => p.IsLand()).ToHashSet();
        Data.Models.Landforms.BuildTris(affectedPolys, Data);
    }
}