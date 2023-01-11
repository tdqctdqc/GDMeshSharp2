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
    }

    

    private void BuildCells()
    {
        var polysPerCell = 3;
        var numCells = Data.GeoPolygons.Count / polysPerCell;
        var landRatio = .33f;
        Data.GeoPolygons.AddRange(Data.GeoPolygons);
        var cellSeeds = GenerationUtility.PickSeeds(Data.GeoPolygons, new int[] {numCells})[0];
        var cells = cellSeeds.Select(p => new GeologyCell(p)).ToList();
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
        var plates = plateSeeds.Select(s => new GeologyPlate(s, _id.GetID())).ToList();
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
        var masses = massSeeds.Select(s => new GeologyMass(s, _id.GetID())).ToList();
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
        var numLandmasses = numMasses / 3;
        var numSeas = (numMasses * 2) / 3;
        if (numLandmasses + numSeas > Data.Masses.Count) throw new Exception();
        var seeds = GenerationUtility.PickSeeds(Data.Masses, new int[] {numLandmasses, numSeas});
        var landSeeds = seeds[0].ToHashSet();
        var waterSeeds = seeds[1].ToHashSet();
        var allSeeds = landSeeds.Union(waterSeeds);
        var conts = landSeeds.Select(s => new Continent(s, _id.GetID(), Root.Random.RandfRange(.6f, .9f)))
            .Union(waterSeeds.Select(s => new Continent(s, _id.GetID(), Root.Random.RandfRange(.1f, .45f))))
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
                poly.SetAltitude(Root.Random.RandfRange(.8f * cont.Altitude, 1.2f * cont.Altitude));
                //todo make this sample perlin
                poly.SetIsLand(poly.Altitude > .5f);
            }
        });
    }

    private void DoContinentFriction()
    {
        Data.Plates.ForEach(p => setFriction(p));
        Data.FaultLines.ForEach(f =>
        {
            f.PolyFootprint.AddRange(getPolysInRangeOfFault(f));   
        });
        
        //find polys in range of fault
        //add altitude + roughness to them
        //union find for the ones that are land and eligible for hill, mtn, etc
        //draw meshes inside these 
        Data.FaultLines.ForEach(f =>
        {
            
        });
        
        
        
        void setFriction(GeologyPlate plate)
        {
            var neighbors = plate.Neighbors.ToList();
            var count = neighbors.Count;
            for (var j = 0; j < count; j++)
            {
                var aPlate = neighbors[j];
                if (aPlate.Id < plate.Id 
                    && aPlate.Mass.Continent != plate.Mass.Continent)
                {
                    var drift1 = plate.Mass.Continent.Drift;
                    var drift2 = aPlate.Mass.Continent.Drift;

                    var axis = aPlate.Center - plate.Center;
                    var driftStr = (drift1 - drift2).Length() / 2f;
                    if (driftStr > .5f)
                    {
                        var borders = plate.GetOrderedBorderSegmentsWithPlate(aPlate);
                        var range = new FaultLine(driftStr, borders, plate, aPlate);
                        Data.FaultLines.Add(range);
                    }
                }
            }
        }

        IEnumerable<GeologyPolygon> getPolysInRangeOfFault(FaultLine fault)
        {
            return fault.HighId.Cells.SelectMany(c => c.PolyGeos).Where(p => fault.PointWithinDist(p.Center, fault.Friction * 200f))
                .Union(fault.LowId.Cells.SelectMany(c => c.PolyGeos).Where(p => fault.PointWithinDist(p.Center, fault.Friction * 200f)));
        }
    }
}