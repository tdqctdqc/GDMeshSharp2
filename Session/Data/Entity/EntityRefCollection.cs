using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class EntityRefCollection<TRef> where TRef : Entity
{
    public HashSet<int> RefIds { get; private set; }
    public IReadOnlyList<TRef> Refs => _refs;

    private List<TRef> _refs;
    
    public EntityRefCollection(IEnumerable<int> refIds, Data data)
    {
        RefIds = refIds.ToHashSet();
        _refs = RefIds.Select(id => (TRef) data[id]).ToList();
    }
    public void Add(int id, Data data)
    {
        //todo need to make this procedure
        RefIds.Add(id);
        _refs.Add((TRef)data[id]);
    }
    public void Remove(int id, Data data)
    {
        //todo need to make this procedure
        RefIds.Remove(id);
        _refs.Remove((TRef)data[id]);

    }
    public void Set(IEnumerable<int> ids, Data data)
    {
        //todo need to make this procedure
        RefIds = ids.ToHashSet();
        _refs = RefIds.Select(id => (TRef) data[id]).ToList();
    }
}
