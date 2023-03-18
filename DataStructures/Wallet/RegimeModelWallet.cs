using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeModelWallet<T> : WalletDic<EntityRef<Regime>, ModelRef<T>> where T : IModel
{
    public static RegimeModelWallet<T> Construct()
    {
        return new RegimeModelWallet<T>(new Dictionary<EntityRef<Regime>, Wallet<ModelRef<T>>>());
    }
    [SerializationConstructor] private RegimeModelWallet(Dictionary<EntityRef<Regime>, Wallet<ModelRef<T>>> wallets) : base(wallets)
    {
    }
}
