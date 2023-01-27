using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta 
{
    IReadOnlyList<string> FieldNames { get; }
    Dictionary<string, Type> FieldTypes { get; }
    // Serializable Deserialize(string json);
    void Initialize(Entity entity, object[] args, ServerWriteKey key);
    object[] GetArgs(Entity entity);
    Entity Deserialize(byte[][] argsBytes, ServerWriteKey key);
    void UpdateEntityVar(string fieldName, Entity t, ServerWriteKey key, object newValue);
    void UpdateEntityVar<TValue>(string fieldName, Entity t, CreateWriteKey key, TValue newValue);
}
