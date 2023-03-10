using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class GeologyGenerator : Generator
{
    public GenData Data { get; private set; }
    private IdDispenser _id;
    private GenWriteKey _key;
    public static readonly float FaultRange = 100f,
        FrictionAltEffect = .03f,
        FrictionRoughnessEffect = 1f;
    public GeologyGenerator()
    {
        
    }
    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _key = key;
        _id = key.IdDispenser;
        Data = key.GenData;
        
        
        
        
        report.StartSection(); 
        BuildCells();
        report.StopSection("BuildCells");
        
        report.StartSection(); 
        BuildPlates();
        report.StopSection("BuildPlates");
        
        report.StartSection(); 
        BuildMasses();
        report.StopSection("BuildMasses");
        
        report.StartSection(); 
        BuildContinents();
        report.StopSection("BuildContinents");
        
        report.StartSection(); 
        DoContinentFriction();
        report.StopSection("DoContinentFriction");
        
        report.StartSection(); 
        HandleIsthmusAndInlandSeas();
        report.StopSection("HandleIsthmusAndInlandSeas");
        
        report.StartSection(); 
        Data.LandSea.SetLandmasses(Data);
        report.StopSection("SetLandmasses");
        
        return report;
    }

    private void HandleIsthmusAndInlandSeas()
    {
        
    }
    
    private void BuildCells()
    {
        var polysPerCell = 3;
        var numCells = Data.Planet.Polygons.Entities.Count / polysPerCell;
        var polyCellDic = Data.GenAuxData.PolyCells;
        var cellSeeds = Picker.PickSeeds(Data.Planet.Polygons.Entities, new int[] {numCells})[0];

        var cells = cellSeeds.Select(p => new GenCell(p, _key, polyCellDic, Data)).ToList();
        Data.GenAuxData.Cells.AddRange(cells);
        var polysNotTaken =
            Data.Planet.Polygons.Entities.Except(cellSeeds);

        var remainder = Picker.PickInTurn(polysNotTaken, 
            cells, 
            cell => cell.NeighboringPolyGeos, 
            (cell, poly) => cell.AddPolygon(poly, _key)
        );
        if (remainder.Count > 0)
        {
            throw new Exception();
        }
        Parallel.ForEach(cells, c => c.SetNeighbors(_key));
    }

    private void BuildPlates()
    {
        var cellsPerPlate = 3;
        var numPlates = Data.GenAuxData.Cells.Count / cellsPerPlate;
        var plateSeeds = Picker.PickSeeds(Data.GenAuxData.Cells, new[] {numPlates})[0];
        var plates = plateSeeds.Select(s => new GenPlate(s, _id.GetID(), _key)).ToList();
        
        Data.GenAuxData.Plates.AddRange(plates);
        var cellsNotTaken = Data.GenAuxData.Cells.Except(plateSeeds);
        var remainder = Picker.PickInTurnHeuristic(cellsNotTaken, plates, 
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
        var massSeeds = Picker.PickSeeds(Data.GenAuxData.Plates, new int[] {numMasses})[0];
        var masses = massSeeds.Select(s => new GenMass(s, _id.GetID())).ToList();

        var platesNotTaken = Data.GenAuxData.Plates.Except(massSeeds);
        var remainder = Picker.PickInTurnHeuristic(platesNotTaken, masses,
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
        var seeds = Picker.PickSeeds(Data.GenAuxData.Masses, new int[] {numLandmasses, numSeas});
        var landSeeds = seeds[0].ToHashSet();
        var waterSeeds = seeds[1].ToHashSet();
        var allSeeds = landSeeds.Union(waterSeeds);
        var conts = landSeeds.Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(.6f, .9f)))
            .Union(waterSeeds.Select(s => new GenContinent(s, _id.GetID(), Game.I.Random.RandfRange(.1f, .45f))))
            .ToList();

        var remainder = Picker.PickInTurn(Data.GenAuxData.Masses.Except(allSeeds), conts,
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
                var altNoise = Data.GenAuxData.GetAltPerlin(poly.Center);
                var altValue = cont.Altitude + .2f * altNoise;
                poly.Set(nameof(MapPolygon.Altitude), altValue, _key);
            }
        });
    }

    private void DoContinentFriction()
    {
        ConcurrentBag<FaultLine> faults = new ConcurrentBag<FaultLine>();
        Parallel.ForEach(Data.GenAuxData.Plates, setFriction);
        foreach (var f in faults)
        {
            Data.GenAuxData.FaultLines.AddFault(f);
        }
        Parallel.ForEach(Data.GenAuxData.FaultLines.FaultLines, f =>
        {
            var inRange = getPolysInRangeOfFault(f);
            f.PolyFootprint.AddRange(inRange);   
        });
        
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
                    if (driftStr > .5f)
                    {
                        var borders = MapPolygon.BorderGraph
                            .GetBorderEdges(hiPlate.Cells.SelectMany(c => c.PolyGeos))
                            .ToList();
                        var friction = driftStr.ProjectToRange(1f, .5f, .5f);
                        var fault = new FaultLine(driftStr, hiPlate, loPlate, borders, Data);
                        faults.Add(fault);
                    }
                }
            }
        }
        

        IEnumerable<MapPolygon> getPolysInRangeOfFault(FaultLine fault)
        {
            var faultRange = fault.Friction * FaultRange;
            var polys = fault.HighId.Cells.SelectMany(c => c.PolyGeos)
                .Union(fault.LowId.Cells.SelectMany(c => c.PolyGeos));
            
            var polysInRange = new List<MapPolygon>();
            foreach (var poly in polys)
            {
                var dist = fault.GetDist(poly, Data);
                var distRatio = (faultRange - dist) / faultRange;
                if (dist < faultRange)
                {
                    polysInRange.Add(poly);
                    var altIncrement = fault.Friction * FrictionAltEffect * distRatio;
                    float erosion = 0f;
                    if (poly.Altitude < .5f) erosion = poly.Altitude;
                    poly.Set(nameof(poly.Altitude), poly.Altitude + altIncrement, _key);
                    
                    var rand = Game.I.Random.RandfRange(-.2f, .2f);
                    var newRoughness = Mathf.Clamp(fault.Friction * FrictionRoughnessEffect * distRatio - erosion + rand, 0f,
                        1f);
                    poly.Set(nameof(poly.Roughness), newRoughness, _key);
                }
            }
            return polysInRange;
        }
    }
}