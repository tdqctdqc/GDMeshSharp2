using Godot;
using System;
using System.Collections.Generic;

public interface IRepo
{
    Domain Domain { get; }
    IReadOnlyCollection<Entity> Entities { get; }
    void AddEntity(Entity e, StrongWriteKey key);
    void RemoveEntity(Entity e, StrongWriteKey key);
    void RaiseValueChangedNotice<TProperty>(string valueName, Entity entity, TProperty oldVal, TProperty newVal, WriteKey key);
}
