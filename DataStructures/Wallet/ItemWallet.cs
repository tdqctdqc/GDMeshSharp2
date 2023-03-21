using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemWallet : Wallet<string>
{
    public float this[Item item] => this[item.Name];
    public static ItemWallet Construct()
    {
        return new ItemWallet(new Dictionary<string, float>());
    }
    [SerializationConstructor] private ItemWallet(Dictionary<string, float> contents) : base(contents)
    {
    }

    public void Add(Item item, float amount)
    {
        Add(item.Name, amount);
    }
    public void Remove(Item item, float amount)
    {
        Remove(item.Name, amount);
    }
}
