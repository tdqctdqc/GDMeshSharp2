using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimeEntityWallet<T> : WalletDic<EntityRef<Regime>, EntityRef<T>>  where T : Entity
{
    public static RegimeEntityWallet<T> Construct()
    {
        return new RegimeEntityWallet<T>(new Dictionary<EntityRef<Regime>, Wallet<EntityRef<T>>>());
    }
    [SerializationConstructor] private RegimeEntityWallet(Dictionary<EntityRef<Regime>, Wallet<EntityRef<T>>> wallets) 
        : base(wallets)
    {
    }
}
