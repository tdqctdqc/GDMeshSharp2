using Godot;
using System;
using System.Collections.Generic;

public interface IRepo
{
    Domain Domain { get; }
    IReadOnlyList<Entity> Entities { get; }
    void AddEntity(Entity e, StrongWriteKey key);
    void RemoveEntity(Entity e, StrongWriteKey key);
    void RaiseValueChangedNotice(string valueName, int id, WriteKey key);
}
