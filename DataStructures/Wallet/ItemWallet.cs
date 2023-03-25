using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemWallet : Wallet<string>
{
    public int this[Item item] => this[item.Name];
    public static ItemWallet Construct()
    {
        return new ItemWallet(new Dictionary<string, int>());
    }
    [SerializationConstructor] private ItemWallet(Dictionary<string, int> contents) : base(contents)
    {
    }

    public void Add(Item item, int amount)
    {
        Add(item.Name, amount);
    }
    public void Remove(Item item, int amount)
    {
        Remove(item.Name, amount);
    }
}
