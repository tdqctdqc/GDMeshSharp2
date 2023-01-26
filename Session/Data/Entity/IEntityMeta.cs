using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta 
{
    IReadOnlyList<string> FieldNames { get; }
    IReadOnlyList<Type> FieldTypes { get; }
    // Serializable Deserialize(string json);
    void Initialize(Entity entity, object[] args);
    object[] GetArgs(Entity entity);
    Entity Deserialize(object[] args);
    void UpdateEntityVar(string fieldName, Entity t, ServerWriteKey key, object newValue);
    void UpdateEntityVar<TValue>(string fieldName, Entity t, CreateWriteKey key, TValue newValue);
    void UpdateEntityRefVar(string fieldName, Entity t, ServerWriteKey key, object newValue);
    void UpdateEntityRefVar<TValue>(string fieldName, Entity t, CreateWriteKey key, TValue newValue);
}
