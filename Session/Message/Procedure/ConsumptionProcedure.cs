
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ConsumptionProcedure : Procedure
{
    public RegimeModelWallet<Item> Consumptions { get; private set; }
    
    public static ConsumptionProcedure Create(RegimeModelWallet<Item> wallets)
    {
        return new ConsumptionProcedure(wallets);
    }
    [SerializationConstructor] 
    private ConsumptionProcedure(RegimeModelWallet<Item> consumptions)
    {
        Consumptions = consumptions;
    }
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var kvp in Consumptions.Wallets)
        {
            var r = kvp.Key.Entity();
            var gains = kvp.Value.Contents;
            foreach (var kvp2 in gains)
            {
                r.Resources.Remove(kvp2.Key.Model(), kvp2.Value);
            }
        }
    }
}
