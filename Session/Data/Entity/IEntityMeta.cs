using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta 
{
    IReadOnlyList<string> FieldNames { get; }
    Dictionary<string, Type> FieldTypes { get; }
    object[] GetPropertyValues(Entity entity);
    void UpdateEntityVarServer<TProperty>(string fieldName, Entity t, ServerWriteKey key, TProperty newValue);
    void UpdateEntityVar<TProperty>(string fieldName, Entity t, StrongWriteKey key, TProperty newValue);
    IRefCollection GetRefCollection(string fieldName, Entity t, ProcedureWriteKey key);
    void AddToData(Entity e, StrongWriteKey key);
    void RemoveFromData(Entity e, StrongWriteKey key);
}
