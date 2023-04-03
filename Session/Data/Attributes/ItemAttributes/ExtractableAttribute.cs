using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ExtractableAttribute : ItemAttribute
{
    public int GetDepletionFromProduction(float size, int production)
    {
        return Mathf.CeilToInt(production / 50);
    }
}
