using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AssetManager
{
    public static void Setup()
    {
        TextureManager.Setup();
        SceneManager.Setup();
        CultureManager.Setup();
    }
}