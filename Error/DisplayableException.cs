
using System;
using Godot;

public abstract class DisplayableException : Exception
{
    public abstract Control GetDisplay();
}
