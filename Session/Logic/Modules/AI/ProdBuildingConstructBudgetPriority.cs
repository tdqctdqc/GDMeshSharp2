using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public class ProdBuildingConstructBudgetPriority : BudgetPriority
{
    public Item ProducedItem { get; private set; }
    public ProdBuildingConstructBudgetPriority(Item producedItem, Func<Data, Regime, float> getWeight) 
        : base(getWeight)
    {
        ProducedItem = producedItem;
    }

    protected float GetDemand(Regime r, Data d)
    {
        var latest = r.History.DemandHistory[ProducedItem.Id].GetLatest();
        return Mathf.Max(100f, latest);
    }

    protected float GetSupply(Regime r, Data d)
    {
        var relevantConstructions = d.Society.CurrentConstruction.ByPoly
            .Where(kvp => d.Planet.Polygons[kvp.Key].Regime.RefId == r.Id)
            .SelectMany(kvp => kvp.Value)
            .Where(c => c.Model.Model() is ProductionBuildingModel)
            .Where(c => ((ProductionBuildingModel)c.Model.Model()).ProdItem == ProducedItem)
            .Sum(c => {
                    var b = ((ProductionBuildingModel) c.Model.Model());
                    return b.ProductionCap;
                }
            );
        return r.History.ProdHistory[ProducedItem.Id].GetLatest() + relevantConstructions;
    }

    public override void Calculate(Regime regime, Data data, ItemWallet budget, Dictionary<Item, float> prices,
        int credit, int availLabor, Action<Message> queueMessage, Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var sw = new Stopwatch();
        sw.Start();
        var buildings = SelectBuildings(regime, data, budget, prices, credit, availLabor);
        sw.Stop();
        var selectBuildingTime = sw.Elapsed.TotalMilliseconds;
        sw.Reset();
        sw.Start();
        SelectBuildSites(regime, data, buildings, budget, queueMessage);
        sw.Stop();
        var selectBuildSiteTime = sw.Elapsed.TotalMilliseconds;
        // GD.Print("select buildings " + selectBuildingTime + " select sites " + selectBuildSiteTime);
    }

    private Dictionary<BuildingModel, int> SelectBuildings(Regime regime, Data data, ItemWallet budget, Dictionary<Item, float> prices,
        float credit, int laborAvail)
    {
        if (laborAvail <= 0) return new Dictionary<BuildingModel, int>();
        var solver = Solver.CreateSolver("GLOP", "CBC_MIXED_INTEGER_PROGRAMMING");
        if (solver is null)
        {
            throw new Exception("solver null");
        }
        var items = data.Models.Items.Models.Select(kvp => kvp.Value.Id).ToList();
        var itemNumConstraints = new Dictionary<int, Constraint>();
        items.ForEach(i =>
        {
            var itemConstraint = solver.MakeConstraint(0f, budget[i]);
            itemNumConstraints.Add(i, itemConstraint);
        });
        
        var creditConstraint = solver.MakeConstraint(0f, credit, "Credits");
        var constructLaborConstraint = solver.MakeConstraint(0, laborAvail, "ConstructLabor");
        var buildingLaborConstraint = solver.MakeConstraint(0, laborAvail, "BuildingLabor");

        
        var buildings = data.Models.Buildings.Models.Values
            .SelectWhereOfType<BuildingModel, ProductionBuildingModel>()
            .Where(pb => pb.ProdItem == ProducedItem);
        var slotConstraints = new Dictionary<BuildingType, Constraint>();
        var slotTypes = buildings.Select(b => b.BuildingType).Distinct();
        foreach (var buildingType in slotTypes)
        {
            var slots = regime.Polygons.Select(p => p.PolyBuildingSlots[buildingType]).Sum();
            var slotConstraint = solver.MakeConstraint(0, slots, buildingType.ToString());
            slotConstraints.Add(buildingType, slotConstraint);
        }
        
        var objective = solver.Objective();
        objective.SetMaximization();
        var projVars = new List<Variable>();
        
        foreach (var b in buildings)
        {
            var projVar = solver.MakeIntVar(0, int.MaxValue, b.Name);
            projVars.Add(projVar);
            foreach (var kvp in b.BuildCosts)
            {
                var item = kvp.Key;
                var num = kvp.Value;
                var itemConstraint = itemNumConstraints[item.Id];
                itemConstraint.SetCoefficient(projVar, num);
            }
            var projPrice = b.BuildCosts.Sum(kvp => prices[kvp.Key] * kvp.Value);
            creditConstraint.SetCoefficient(projVar, projPrice);
            constructLaborConstraint.SetCoefficient(projVar, b.LaborPerTickToBuild);
            buildingLaborConstraint.SetCoefficient(projVar, b.TotalLaborReq());
            var slotConstraint = slotConstraints[b.BuildingType];
            slotConstraint.SetCoefficient(projVar, 1);
            
            var benefit = b.ProductionCap;
            objective.SetCoefficient(projVar, benefit);
        }
        var status = solver.Solve();
        
        var res = new Dictionary<BuildingModel, int>();
        foreach (var projVar in projVars)
        {
            var num = (int)projVar.SolutionValue();
            var proj = buildings.First(p => p.Name == projVar.Name());
            res.Add(proj, num);
        }

        return res;
    }
    private void SelectBuildSites(Regime regime, Data data, Dictionary<BuildingModel, int> toBuild, 
        ItemWallet budget, Action<Message> queueMessage)
    {
        var currConstruction = data.Society.CurrentConstruction;
        var availPolys = regime.Polygons;
        var newConstructionPoses = new HashSet<PolyTriPosition>();
        foreach (var kvp in toBuild)
        {
            var building = kvp.Key;
            var num = kvp.Value;
            for (var i = 0; i < num; i++)
            {
                MapPolygon poly = null;
                poly = availPolys
                    .FirstOrDefault(p => p.PolyBuildingSlots[building.BuildingType] > 0);
                if (poly == null) continue;
                var slots = poly.PolyBuildingSlots.AvailableSlots[building.BuildingType]
                    .Where(pt => newConstructionPoses.Contains(pt) == false);
                
                if (slots.Count() == 0) continue;
                
                var pos = slots.First();
                newConstructionPoses.Add(pos);

                var proc = StartConstructionProcedure.Construct(
                    building.MakeRef<BuildingModel>(),
                    pos,
                    regime.MakeRef()
                );
                foreach (var buildCost in building.BuildCosts)
                {
                    budget.Remove(buildCost.Key, buildCost.Value);
                }
                queueMessage(proc);
            }
        }
    }
}
