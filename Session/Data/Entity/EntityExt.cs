using Godot;
using System;
using System.Collections.Generic;

public static class EntityExt
{
    public static EntityRef<T> MakeRef<T>(this T t) where T : Entity
    {
        //todo make static / sharable
        return new EntityRef<T>(t.Id);
    }
}
