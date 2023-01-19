using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class EntityRefCollection<TRef> where TRef : Entity
{
    public string Name { get; private set; }
    public int EntityId { get; private set; }
    public IReadOnlyCollection<int> Ids => _col;
    public IEnumerable<TRef> Refs(Data data) => GetEnumerable(data);
    private HashSet<int> _col;

    public EntityRefCollection(ICollection<int> ids, string name, int entityId)
    {
        _col = ids.ToHashSet();
        Name = name;
        EntityId = entityId;
    }
    private IEnumerable<TRef> GetEnumerable(Data data)
    {
        _col.RemoveWhere(id => (TRef) data[id] == null);
        return _col.Select(id => (TRef) data[id]);
    }

    public static EntityRefCollection<TRef> Construct(ICollection<int> ids, Entity entity, string name)
    {
        return new EntityRefCollection<TRef>(ids, name, entity.Id.Value);
    }
    
    public static string Serialize(EntityRefCollection<TRef> es)
    {
        return JsonSerializer.Serialize<HashSet<int>>(es._col);
    }

    public static EntityRefCollection<TRef> Deserialize(string json, string name, Entity entity)
    {
        var value = JsonSerializer.Deserialize<HashSet<int>>(json);
        return Construct(value, entity, name);
    }
    
    public static void ReceiveUpdate(EntityRefCollection<TRef> str, ServerWriteKey key, string newValueJson)
    {
        var value = JsonSerializer.Deserialize<HashSet<int>>(newValueJson);
        str._col = value;
        key.Data.EntityRepos[str.EntityId].RaiseValueChangedNotice(str.Name, str.EntityId, key);
    }
    
    public void SetByProcedure(ProcedureWriteKey key, HashSet<int> newValue)
    {
        _col = newValue;
    }
}
