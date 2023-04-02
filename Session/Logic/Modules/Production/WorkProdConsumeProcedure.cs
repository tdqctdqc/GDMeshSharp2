
using System.Linq;
using System.Collections.Generic;
using Godot;
using MessagePack;

public class WorkProdConsumeProcedure : Procedure
{
    public Dictionary<int, ItemWallet> RegimeResourceGains { get; private set; }
    public Dictionary<int, EntityWallet<ResourceDeposit>> Depletions { get; private set; }
    public Dictionary<int, ItemWallet> ConsumptionsByRegime { get; private set; }
    public Dictionary<int, ItemWallet> DemandsByRegime { get; private set; }
    public Dictionary<int, EmploymentReport> EmploymentReports { get; private set; }

    public static WorkProdConsumeProcedure Create()
    {
        return new WorkProdConsumeProcedure(new Dictionary<int, ItemWallet>(), 
            new Dictionary<int, EntityWallet<ResourceDeposit>>(), 
            new Dictionary<int, ItemWallet>(), new Dictionary<int, ItemWallet>(),
            new Dictionary<int, EmploymentReport>());
    }
    [SerializationConstructor] private WorkProdConsumeProcedure(
        Dictionary<int, ItemWallet> regimeResourceGains, 
        Dictionary<int, EntityWallet<ResourceDeposit>> depletions,
        Dictionary<int, ItemWallet> consumptionsByRegime,
        Dictionary<int, ItemWallet> demandsByRegime,
        Dictionary<int, EmploymentReport> employmentReports)
    {
        RegimeResourceGains = regimeResourceGains;
        Depletions = depletions;
        ConsumptionsByRegime = consumptionsByRegime;
        DemandsByRegime = demandsByRegime;
        EmploymentReports = employmentReports;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        EnactProduce(key);
        EnactConsume(key);
        foreach (var kvp in EmploymentReports)
        {
            var poly = key.Data.Planet.Polygons[kvp.Key];
            poly.SetEmploymentReport(kvp.Value, key);
        }
    }

    private void EnactProduce(ProcedureWriteKey key)
    {
        var tick = key.Data.Tick;
        foreach (var kvp in RegimeResourceGains)
        {
            var r = (Regime)key.Data[kvp.Key];
            var gains = kvp.Value;
            var snapshot = gains.GetSnapshot();
            foreach (var kvp2 in gains.Contents)
            {
                var item = key.Data.Models.Items.Models[kvp2.Key];
                r.Items.Add(item, kvp2.Value);
            }
            r.History.ProdHistory.AddSnapshot(tick, snapshot, key);
        }

        foreach (var kvp in Depletions)
        {
            var losses = kvp.Value;
            foreach (var kvp2 in losses.Contents)
            {
                var deposit = (ResourceDeposit) key.Data[kvp2.Key];
                var loss = kvp2.Value;
                var newSize = Mathf.Max(0, deposit.Size - loss);
                deposit.Set<int>(nameof(deposit.Size), newSize, key);
            }
        }
    }
    private void EnactConsume(ProcedureWriteKey key)
    {
        var tick = key.Data.BaseDomain.GameClock.Tick;
        foreach (var kvp in ConsumptionsByRegime)
        {
            var r = (Regime)key.Data[kvp.Key];
            var gains = kvp.Value.Contents;
            var snapshot = kvp.Value.GetSnapshot();

            foreach (var kvp2 in gains)
            {
                var model = key.Data.Models.Items.Models[kvp2.Key];
                r.Items.Remove(model, kvp2.Value);
            }
            r.History.ConsumptionHistory.AddSnapshot(tick, snapshot, key);
        }
        foreach (var kvp in DemandsByRegime)
        {
            var r = (Regime)key.Data[kvp.Key];
            var demands = kvp.Value.Contents;
            var snapshot = kvp.Value.GetSnapshot();
            r.History.DemandHistory.AddSnapshot(tick, snapshot, key);

        }
    }
}
