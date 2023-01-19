using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorNetStd;

public class GeologyGenerator
{
    public WorldData Data { get; private set; }
    private IDDispenser _id;
    public GeologyGenerator(WorldData data)
    {
        _id = new IDDispenser();
        Data = data;
    }
    public void GenerateTerrain()
    {
        EdgeDisturber.DisturbEdges(Data.GeoPolygons, Data.Dimensions);
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
        var numCells = Data.GeoPolygons.Count / polysPerCell;
        var landRatio = .33f;
        Data.GeoPolygons.AddRange(Data.GeoPolygons);
        var cellSeeds = GenerationUtility.PickSeeds(Data.GeoPolygons, new int[] {numCells})[0];
        var cells = cellSeeds.Select(p => new GenCell(p)).ToList();
        Data.Cells.AddRange(cells);
        GD.Print("Num cells: " + cells.Count);
        var polysNotTaken =
            Data.GeoPolygons.Except(cellSeeds);
        GenerationUtility.PickInTurn(polysNotTaken, cells, 
            cell => cell.NeighboringPolyGeos, 
            (cell, terrain) => cell.AddPolygon(terrain));
        cells.ForEach(c => c.SetNeighbors());
    }

    private void BuildPlates()
    {
        var cellsPerPlate = 3;
        var numPlates = Data.Cells.Count / cellsPerPlate;
        var plateSeeds = GenerationUtility.PickSeeds(Data.Cells, new[] {numPlates})[0];
        var plates = plateSeeds.Select(s => new GenPlate(s, _id.GetID())).ToList();
        GD.Print("Num plates: " + plates.Count);

        Data.Plates.AddRange(plates);
        var cellsNotTaken = Data.Cells.Except(plateSeeds);
        GenerationUtility.PickInTurnHeuristic(cellsNotTaken, plates, 
            plate => plate.NeighboringCells,
            (plate, cell) => plate.AddCell(cell),
            ((cell, plate) => plate.NeighboringCellsAdjCount[cell]));
        
        plates.ForEach(p => p.SetNeighbors());
    }

    private void BuildMasses()
    {
        var platesPerMass = 3;
        var numMasses = Data.Plates.Count / 3;
        var massSeeds = GenerationUtility.PickSeeds(Data.Plates, new int[] {numMasses})[0];
        var masses = massSeeds.Select(s => new GenMass(s, _id.GetID())).ToList();
        GD.Print("Num masses: " + masses.Count);

        var platesNotTaken = Data.Plates.Except(massSeeds);
        GenerationUtility.PickInTurnHeuristic(platesNotTaken, masses,
            mass => mass.NeighboringPlates,
            (mass, plate) => mass.AddPlate(plate),
            (plate, mass) => mass.NeighboringPlatesAdjCount[plate]);
        Data.Masses.AddRange(masses);
        masses.ForEach(m => m.SetNeighbors());
    }

    private void BuildContinents()
    {
        var massesPerCont = 3;
        var numMasses = Data.Masses.Count / 3;
        var numLandmasses = numMasses / 4;
        var numSeas = numMasses * 3 / 4;
        if (numLandmasses + numSeas > Data.Masses.Count) throw new Exception();
        var seeds = GenerationUtility.PickSeeds(Data.Masses, new int[] {numLandmasses, numSeas});
        var landSeeds = seeds[0].ToHashSet();
        var waterSeeds = seeds[1].ToHashSet();
        var allSeeds = landSeeds.Union(waterSeeds);
        var conts = landSeeds.Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(.6f, .9f)))
            .Union(waterSeeds.Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(.1f, .45f))))
            .ToList();
        GD.Print("Num conts: " + conts.Count);

        GenerationUtility.PickInTurn(Data.Masses.Except(allSeeds), conts,
            cont => cont.NeighboringMasses,
            (cont, mass) => cont.AddMass(mass));
        Data.Continents.AddRange(conts);
        conts.ForEach(c => c.SetNeighbors());
        Data.Continents.ForEach(cont =>
        {
            var isLand = landSeeds.Contains(cont.Seed);
            var polys = cont.Masses
                .SelectMany(m => m.Plates)
                .SelectMany(p => p.Cells)
                .SelectMany(c => c.PolyGeos);
            foreach (var poly in polys)
            {
                poly.SetAltitude(Game.I.Random.RandfRange(.8f * cont.Altitude, 1.2f * cont.Altitude));
                //todo make this sample perlin
            }
        });
    }

    private void DoContinentFriction()
    {
        Data.Plates.ForEach(p => setFriction(p));
        Data.FaultLines.ForEach(f =>
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
                        var borders = hiPlate.GetOrderedBorderRelative(loPlate);
                        var fault = new FaultLine(driftStr, hiPlate, loPlate, borders, Data);
                        Data.FaultLines.Add(fault);
                    }
                }
            }
        }

        IEnumerable<GenPolygon> getPolysInRangeOfFault(FaultLine fault)
        {
            var faultRange = fault.Friction * 500f;
            var polys = fault.HighId.Cells.SelectMany(c => c.PolyGeos)
                .Union(fault.LowId.Cells.SelectMany(c => c.PolyGeos));
            var frictionAltEffect = .1f;
            var frictionRoughnessEffect = 1f;
            var polysInRange = new List<GenPolygon>();
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
                    poly.SetAltitude(poly.Altitude + altIncrement);
                    var rand = Game.I.Random.RandfRange(-.2f, .2f);
                    var newRoughness = Mathf.Clamp(fault.Friction * frictionRoughnessEffect * distRatio - erosion + rand, 0f,
                        1f);
                    poly.SetRoughness(newRoughness);
                }
            }

            return polysInRange;
        }
    }

    private void BuildLandformTris()
    {
        var affectedPolys = Data.FaultLines.SelectMany(f => f.PolyFootprint).Where(p => p.IsLand()).ToHashSet();
        Data.Landforms.BuildTris(affectedPolys, Data);
    }
}