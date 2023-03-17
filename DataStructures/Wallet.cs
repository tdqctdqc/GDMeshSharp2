
using System;
using System.Collections.Generic;
using MessagePack;

public class Wallet<T>
{
    public Dictionary<T, float> Contents { get; private set; }

    [SerializationConstructor] public Wallet(Dictionary<T, float> contents)
    {
        Contents = contents;
    }

    public void Add(T t, float amount, StrongWriteKey key)
    {
        if (amount < 0f) throw new Exception("Trying to add negative amount to wallet");
        if(Contents.ContainsKey(t) == false) Contents.Add(t, 0f);
        Contents[t] += amount;
    }
    public void Remove(T t, float amount, StrongWriteKey key)
    {
        if (amount < 0f) throw new Exception("Trying to remove negative amount from wallet");
        if(Contents.ContainsKey(t) == false) throw new Exception("Trying to remove whats not in wallet");
        if(Contents[t] < amount) throw new Exception("Trying to remove more than in wallet");
        Contents[t] -= amount;
    }

    public void Transfer<R>(R t, float amount, Wallet<R> destination, StrongWriteKey key) where R : T
    {
        Remove(t, amount, key);
        destination.Add(t, amount, key);
    }
}
