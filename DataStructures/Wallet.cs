
using System;
using System.Collections.Generic;
using MessagePack;

public class Wallet<T>
{
    public IReadOnlyDictionary<T, float> Contents => _contents;
    private Dictionary<T, float> _contents;

    [SerializationConstructor] public Wallet(Dictionary<T, float> contents)
    {
        _contents = contents;
    }

    public Wallet()
    {
        _contents = new Dictionary<T, float>();
    }

    public void Add(T t, float amount)
    {
        if (amount < 0f) throw new Exception("Trying to add negative amount to wallet");
        if(_contents.ContainsKey(t) == false) _contents.Add(t, 0f);
        _contents[t] += amount;
    }
    public void Remove(T t, float amount)
    {
        if (amount < 0f) throw new Exception("Trying to remove negative amount from wallet");
        if(_contents.ContainsKey(t) == false) throw new Exception("Trying to remove whats not in wallet");
        if(_contents[t] < amount) throw new Exception("Trying to remove more than in wallet");
        _contents[t] -= amount;
    }

    public void Transfer<R>(R t, float amount, Wallet<R> destination) where R : T
    {
        Remove(t, amount);
        destination.Add(t, amount);
    }
}
