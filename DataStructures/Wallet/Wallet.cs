
using System;
using System.Collections.Generic;
using Godot;
using MessagePack;

public abstract class Wallet<T>
{
    public Dictionary<T, float> Contents { get; private set; }
    public float this[T t] => Contents.ContainsKey(t) ? Contents[t] : 0f;
    [SerializationConstructor] protected Wallet(Dictionary<T, float> contents)
    {
        Contents = contents;
    }
    public Snapshot<T, float> GetSnapshot()
    {
        return Snapshot<T, float>.Construct(Contents);
    }
    protected void Add(T t, float amount)
    {
        if (amount < 0f) throw new Exception("Trying to add negative amount to wallet");
        if(Contents.ContainsKey(t) == false) Contents.Add(t, 0f);
        Contents[t] += amount;
    }
    protected void Remove(T t, float amount)
    {
        if (amount < 0f) throw new Exception("Trying to remove negative amount from wallet");
        if(Contents.ContainsKey(t) == false)
        {
            return;
            throw new Exception("Trying to remove whats not in wallet");
        }
        if(Contents[t] < amount) throw new Exception("Trying to remove more than in wallet");
        Contents[t] -= amount;
    }

    public void TransferFrom<R>(R t, float amount, Wallet<R> destination) where R : T
    {
        Remove(t, amount);
        destination.Add(t, amount);
    }
}
