using Godot;
using System;
using System.Collections.Generic;

public class IDDispenser
{
    private int _index = 0;
    public int GetID()
    {
        int id = _index;
        _index++;
        return id;
    }
}