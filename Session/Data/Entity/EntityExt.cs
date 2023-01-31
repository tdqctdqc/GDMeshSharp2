using Godot;
using System;
using System.Collections.Generic;

public static class EntityExt
{
    public static void Log(this Entity e, int outOf, string msg)
    {
        if (e.Id % outOf == 0)
        {
            GD.Print(msg);
        }
    }
}
