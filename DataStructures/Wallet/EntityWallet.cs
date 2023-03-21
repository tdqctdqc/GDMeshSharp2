using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntityWallet<T> : Wallet<int> 
    where T : Entity
{
    public static EntityWallet<T> Construct()
    {
        return new EntityWallet<T>(new Dictionary<int, float>());
    }
    [SerializationConstructor] private EntityWallet(Dictionary<int, float> contents) : base(contents)
    {
    }
    
    public void Add(T t, float amount)
    {
        Add(t.Id, amount);
    }
    public void Remove(T t, float amount)
    {
        Remove(t.Id, amount);
    }
}
