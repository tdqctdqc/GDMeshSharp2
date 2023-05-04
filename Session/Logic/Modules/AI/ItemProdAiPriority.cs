using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;


public class ItemProdAiPriority : AiPriority
{
    public Item Item { get; private set; }
    public ItemProdAiPriority(Item item, float weight) : base(weight)
    {
        Item = item;
    }

    protected override float GetDemand(Regime r, Data d)
    {
        var latest = r.History.DemandHistory[Item.Name].GetLatest();
        return Mathf.Max(100f, latest);
    }

    protected override float GetSupply(Regime r, Data d)
    {
        var relevantConstructions = d.Society.CurrentConstruction.ByPoly
            .Where(kvp => d.Planet.Polygons[kvp.Key].Regime.RefId == r.Id)
            .SelectMany(kvp => kvp.Value)
            .Where(c => c.Model.Model() is ProductionBuildingModel)
            .Where(c => ((ProductionBuildingModel)c.Model.Model()).ProdItem == Item)
            .Sum(c => {
                    var b = ((ProductionBuildingModel) c.Model.Model());
                    return b.ProductionCap * b.GetPolyEfficiencyScore(c.Pos.Poly(d), d);
                }
            );
        return r.History.ProdHistory[Item.Name].GetLatest() + relevantConstructions;
    }

    public override void Calculate(Regime regime, Data data, ItemWallet budget, Dictionary<Item, float> prices,
        int credit, Action<Message> queueMessage, Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var availLabor = regime.Polygons.Sum(p => p.Employment.NumUnemployed() 
                                                  + p.Employment.NumJob(PeepJobManager.Gatherer) / 2);
        var buildings = SelectBuildings(data, budget, prices, credit, availLabor);
        SelectBuildSites(regime, data, buildings, queueMessage);
    }

    private Dictionary<BuildingModel, int> SelectBuildings(Data data, ItemWallet budget, Dictionary<Item, float> prices,
        float credit, int laborAvail)
    {
        if (laborAvail <= 0) return new Dictionary<BuildingModel, int>();
        var solver = Solver.CreateSolver("GLOP", "CBC_MIXED_INTEGER_PROGRAMMING");
        if (solver is null)
        {
            throw new Exception("solver null");
        }
        var items = data.Models.Items.Models.Select(kvp => kvp.Key).ToList();
        var itemNumConstraints = new Dictionary<string, Constraint>();
        items.ForEach(i =>
        {
            var itemConstraint = solver.MakeConstraint(0f, budget[i]);
            itemNumConstraints.Add(i, itemConstraint);
        });
        var creditConstraint = solver.MakeConstraint(0f, credit, "Credits");
        var constructLaborConstraint = solver.MakeConstraint(0, laborAvail, "ConstructLabor");
        var buildingLaborConstraint = solver.MakeConstraint(0, laborAvail, "BuildingLabor");
        
        
        var objective = solver.Objective();
        objective.SetMaximization();
        var projVars = new List<Variable>();

        var buildings = data.Models.Buildings.Models.Values
            .SelectWhereOfType<BuildingModel, ProductionBuildingModel>()
            .Where(pb => pb.ProdItem == Item);
        foreach (var b in buildings)
        {
            var projVar = solver.MakeIntVar(0, int.MaxValue, b.Name);
            projVars.Add(projVar);
            foreach (var kvp in b.BuildCosts)
            {
                var item = kvp.Key;
                var num = kvp.Value;
                var itemConstraint = itemNumConstraints[item.Name];
                itemConstraint.SetCoefficient(projVar, num);
            }
            var projPrice = b.BuildCosts.Sum(kvp => prices[kvp.Key] * kvp.Value);
            creditConstraint.SetCoefficient(projVar, projPrice);
            constructLaborConstraint.SetCoefficient(projVar, b.LaborPerTickToBuild);
            buildingLaborConstraint.SetCoefficient(projVar, b.TotalLaborReq());

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
            
            foreach (var kvp in proj.BuildCosts)
            {
                // budget.Remove(kvp.Key, kvp.Value);
            }
        }

        return res;
    }
    private void SelectBuildSites(Regime regime, Data data, Dictionary<BuildingModel, int> toBuild, Action<Message> queueMessage)
    {
        var currConstruction = data.Society.CurrentConstruction;
        var availPolys = regime.Polygons
            .Where(p => currConstruction.ByPoly.ContainsKey(p.Id) == false)
            .Where(p => p.GetMapBuildings(data) == null || p.GetMapBuildings(data).Count < p.GetNumAllowedBuildings())
            .Where(p => p.Tris.Tris.Any(t => data.Society.BuildingAux.ByTri.ContainsKey(t) == false))
            .ToHashSet();
        foreach (var kvp in toBuild)
        {
            if (availPolys.Count == 0) break;
            var building = kvp.Key;
            var num = kvp.Value;
            for (var i = 0; i < num; i++)
            {
                MapPolygon poly = null;
                if (availPolys.Count == 0) break;
                poly = availPolys
                    .OrderByDescending(p => building.GetPolyEfficiencyScore(p, data))
                    .First();
                var tri = poly.Tris.Tris
                    .First(t => data.Society.BuildingAux.ByTri.ContainsKey(t) == false);
                availPolys.Remove(poly);

                var proc = StartConstructionProcedure.Construct(
                    building.MakeRef<BuildingModel>(),
                    new PolyTriPosition(poly.Id, tri.Index),
                    regime.MakeRef()
                );
                queueMessage(proc);
            }
        }
    }
}
