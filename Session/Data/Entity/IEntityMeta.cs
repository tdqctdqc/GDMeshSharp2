using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta 
{
    IReadOnlyList<string> FieldNames { get; }
    Dictionary<string, Type> FieldTypes { get; }
    object[] GetPropertyValues(Entity entity);
    void UpdateEntityVar<TProperty>(string fieldName, Entity t, ServerWriteKey key, TProperty newValue);
    void UpdateEntityVar<TProperty>(string fieldName, Entity t, CreateWriteKey key, TProperty newValue);
}
