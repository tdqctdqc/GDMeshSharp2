using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SingletonRepo<T> : Repository<T> where T : Entity
{
    public T Value => Entities[0];
    public SingletonRepo(Domain domain, Data data) : base(domain, data)
    {
        AddedEntity += (entity, key) =>
        {
            if (Entities.Count > 1) throw new Exception();
        };
    }
}