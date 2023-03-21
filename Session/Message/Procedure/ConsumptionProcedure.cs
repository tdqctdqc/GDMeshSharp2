
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ConsumptionProcedure : Procedure
{
    public Dictionary<int, ItemWallet> ConsumptionsByRegime { get; private set; }
    
    public static ConsumptionProcedure Create(Dictionary<int, ItemWallet> byRegime)
    {
        return new ConsumptionProcedure(byRegime);
    }
    [SerializationConstructor] 
    private ConsumptionProcedure(Dictionary<int, ItemWallet> consumptionsByRegime)
    {
        ConsumptionsByRegime = consumptionsByRegime;
    }
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var tick = key.Data.BaseDomain.GameClock.Value.Tick;
        foreach (var kvp in ConsumptionsByRegime)
        {
            var r = (Regime)key.Data[kvp.Key];
            var gains = kvp.Value.Contents;
            var snapshot = kvp.Value.GetSnapshot();

            foreach (var kvp2 in gains)
            {
                var model = key.Data.Models.Items.Models[kvp2.Key];
                r.Resources.Remove(model, kvp2.Value);
            }
            r.ConsumptionHistory.AddSnapshot(tick, snapshot, key);
        }
    }
}
