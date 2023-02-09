using System;
using System.Collections.Generic;
using Godot;


public class GenerationParameters
{
    public Vector2 Dimensions { get; private set; }

    public GenerationParameters(Vector2 dimensions)
    {
        Dimensions = dimensions;
    }
}
