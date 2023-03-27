using Godot;
using System;
using System.Collections.Generic;

public interface IEntityMeta
{
    Type EntityType { get; }
    Type DomainType { get; }
    IReadOnlyList<string> FieldNameList { get; }
    HashSet<string> FieldNameHash { get; }
    IReadOnlyDictionary<string, Type> FieldTypes { get; }
    object[] GetPropertyValues(Entity entity);
    void UpdateEntityVarServer<TProperty>(string fieldName, Entity t, ServerWriteKey key, TProperty newValue);
    void UpdateEntityVar<TProperty>(string fieldName, Entity t, StrongWriteKey key, TProperty newValue);
    IRefCollection GetRefCollection(string fieldName, Entity t, ProcedureWriteKey key);
    bool TestSerialization(Entity e);
}
