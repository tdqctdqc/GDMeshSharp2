using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta
{
    IReadOnlyList<string> FieldNames { get; }
    IReadOnlyDictionary<string, Type> FieldTypes { get; }
    // Serializable Deserialize(string json);
    void Initialize(Entity entity, string json);
    string Serialize(Entity entity);
    Entity Deserialize(string json);
    void UpdateEntityVar(string fieldName, Entity t, ServerWriteKey key, string newValueJson);
}
