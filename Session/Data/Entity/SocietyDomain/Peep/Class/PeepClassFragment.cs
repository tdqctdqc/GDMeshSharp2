using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepClassFragment
{
    public int Size { get; private set; }
    public ModelRef<PeepClass> PeepClass { get; private set; }
    
    public PeepClassFragment(int size, ModelRef<PeepClass> peepClass)
    {
        Size = size;
        PeepClass = peepClass;
    }

    public void Grow(int delta)
    {
        Size += delta;
    }

    public void Shrink(int delta)
    {
        Size = Mathf.Max(0, Size - delta);
    }
}
