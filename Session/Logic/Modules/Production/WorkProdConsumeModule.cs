
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class WorkProdConsumeModule : LogicModule
{
    private int _lastRunTick;
    private int _ticksSinceLast;
    private ConcurrentDictionary<int, EntityWallet<ResourceDeposit>> 
        _regimeDepletionWallets = new ConcurrentDictionary<int, EntityWallet<ResourceDeposit>>();
    private ConcurrentDictionary<int, ItemWallet> 
        _regimeProdWallets = new ConcurrentDictionary<int, ItemWallet>();
    private ConcurrentDictionary<int, ItemWallet> 
        _regimeConsWallets = new ConcurrentDictionary<int, ItemWallet>();
    private ConcurrentDictionary<int, ItemWallet> 
        _regimeDemandWallets = new ConcurrentDictionary<int, ItemWallet>();
    private ConcurrentDictionary<int, EmploymentReport> 
        _polyEmployReps = new ConcurrentDictionary<int, EmploymentReport>();
    private void Clear()
    {
        foreach (var kvp in _regimeDepletionWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _regimeProdWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _regimeConsWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _regimeDemandWallets) { kvp.Value.Clear(); }
        foreach (var kvp in _polyEmployReps) { kvp.Value.Counts.Clear(); }
    }
    public override void Calculate(Data data, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        Clear();
        var tick = data.BaseDomain.GameClock.Tick;
        _ticksSinceLast = tick - _lastRunTick;
        _lastRunTick = tick;
        var proc = WorkProdConsumeProcedure.Create(_ticksSinceLast);

        Parallel.ForEach(data.Society.Regimes.Entities, 
            regime => ProduceForRegime(regime, data, proc));
        
        Parallel.ForEach(data.Society.Regimes.Entities,
            regime => ConsumeForRegime(proc, regime, data));

        queueMessage(proc);
    }

    private void ProduceForRegime(Regime regime, Data data, WorkProdConsumeProcedure proc)
    {
        var gains = _regimeProdWallets.GetOrAdd(regime.Id,
            id => ItemWallet.Construct());
        proc.RegimeResourceGains.TryAdd(regime.Id, gains);

        var depletions = _regimeDepletionWallets.GetOrAdd(regime.Id, 
            id => EntityWallet<ResourceDeposit>.Construct());
        proc.Depletions.TryAdd(regime.Id, depletions);
        var polys = regime.Polygons;
        
        foreach (var poly in polys)
        {
            ProduceForPoly(poly, proc, data);
        }
    }

    private void ProduceForPoly(MapPolygon poly, WorkProdConsumeProcedure proc, Data data)
    {
        var peeps = poly.GetPeeps(data);
        if (peeps == null) return;
        
        var mapBuildings = poly.GetMapBuildings(data);
        if (mapBuildings == null || mapBuildings.Count == 0) return;
        
        var workBuildings = mapBuildings.Where(b => b.Model.Model() is WorkBuildingModel)
                    .Select(b => (WorkBuildingModel)b.Model.Model());
        
        IEnumerable<WorkBuildingModel> settlementBuildings = null;
        if (poly.HasSettlement(data))
        {
            settlementBuildings = poly.GetSettlement(data)
                .Buildings.Models().SelectWhereOfType<BuildingModel, WorkBuildingModel>();
            workBuildings = workBuildings.Union(settlementBuildings);
        }

        var jobNeeds = workBuildings.SelectMany(b => b.JobLaborReqs);
        IEnumerable<Construction> constructions = null;
        if (poly.Regime.Empty() == false)
        {        
            var r = poly.Regime.Entity();
            if (data.Society.CurrentConstruction.GetPolyConstructions(poly) is List<Construction> cs)
            {
                constructions = cs;
                var constructionNeed = cs.Select(c => c.Model.Model().LaborPerTickToBuild).Sum();
                jobNeeds = jobNeeds.Append(
                    new KeyValuePair<PeepJobAttribute, int>(PeepJobAttribute.ConstructionAttribute, constructionNeed));
            }
        }
        
        var jobNeedCount = jobNeeds.Sum(kvp => kvp.Value);
        if (jobNeedCount == 0) return;
        var peepsCount = peeps.Sum(p => p.Size);
        var ratio = (float) peepsCount / (float) jobNeedCount;
        ratio = Mathf.Clamp(ratio, 0f, 1f);
        
        var employment = _polyEmployReps.GetOrAdd(poly.Id, p => EmploymentReport.Construct());
        var unemployed = peepsCount;
        foreach (var kvp in jobNeeds)
        {
            var att = kvp.Key;
            var needed = kvp.Value;
            var job = data.Models.PeepJobs.Models.First(kvp2 => kvp2.Value.Attributes.Has(att)).Value;
            var hire = Mathf.Min(Mathf.CeilToInt(needed * ratio), unemployed);
            unemployed -= hire;
            employment.Counts.AddOrSum(job.Name, hire);
            if (unemployed == 0) break;
        }
        
        employment.Counts[PeepJobManager.Unemployed.Name] = unemployed;
        proc.EmploymentReports.TryAdd(poly.Id, employment);
        
        foreach (var building in mapBuildings)
        {
            if (building.Model.Model() is WorkBuildingModel wb)
            {
                wb.Produce(proc, building.Position, ratio, _ticksSinceLast, data);
            }
        }

        if (settlementBuildings != null)
        {
            var tri = poly.Tris.Tris.First(t => t.Landform == LandformManager.Urban);
            var pos = new PolyTriPosition(poly.Id, tri.Index);
            foreach (var wb in settlementBuildings)
            {
                wb.Produce(proc, pos, ratio, _ticksSinceLast, data);
            }
        }

        if (constructions != null)
        {
            foreach (var construction in constructions)
            {
                proc.ConstructionProgresses.TryAdd(construction.Pos, ratio);
            }
        }
    }

    private void ConsumeForRegime(WorkProdConsumeProcedure proc, Regime regime, Data data)
    {
        var consumptions = _regimeConsWallets.GetOrAdd(regime.Id,
            id => ItemWallet.Construct());
        proc.ConsumptionsByRegime.TryAdd(regime.Id, consumptions);

        var demands = _regimeDemandWallets.GetOrAdd(regime.Id,
            id => ItemWallet.Construct());
        proc.DemandsByRegime.TryAdd(regime.Id, demands);
        
        var numPeeps = regime.Polygons
            .Where(p => p.GetPeeps(data) != null)
            .SelectMany(p => p.GetPeeps(data)).Sum(p => p.Size);
        var foodDesired = numPeeps * data.BaseDomain.Rules.FoodConsumptionPerPeepPoint * _ticksSinceLast;
        demands.Add(ItemManager.Food, foodDesired);
        var foodStock = regime.Items[ItemManager.Food] + proc.RegimeResourceGains[regime.Id][ItemManager.Food];
        var foodConsumption = Mathf.Min(foodDesired, foodStock);
        consumptions.Add(ItemManager.Food, foodConsumption);
    }
}
