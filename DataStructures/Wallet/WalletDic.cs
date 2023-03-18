using System;
using System.Collections.Generic;
using System.Linq;

public class WalletDic<TKey, TValue>
{
    public Dictionary<TKey, Wallet<TValue>> Wallets;
    public Wallet<TValue> this[TKey key] => Wallets[key];

    public static WalletDic<TKey, TValue> Construct()
    {
        return new WalletDic<TKey, TValue>(new Dictionary<TKey, Wallet<TValue>>());
    }
    public WalletDic(Dictionary<TKey, Wallet<TValue>> wallets)
    {
        Wallets = new Dictionary<TKey, Wallet<TValue>>();
    }

    public Wallet<TValue> AddOrGet(TKey key)
    {
        if (Wallets.ContainsKey(key) == false) Wallets.Add(key, Wallet<TValue>.Construct());
        return Wallets[key];
    }
}
