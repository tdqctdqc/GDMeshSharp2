using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SingletonAux<T> : EntityAux<T> where T : Entity
{
    public T Value => Register.Entities.FirstOrDefault();
    public SingletonAux(Domain domain, Data data) : base(domain, data)
    {
        EntityCreatedHandler<T>.Register(
            entity => { if (Register.Entities.Count > 1) throw new Exception(); }
        );
    }
}