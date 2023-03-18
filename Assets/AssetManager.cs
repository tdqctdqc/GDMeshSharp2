using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AssetManager
{
    public static void Setup()
    {
        GD.Print("setting up assets");
        TextureManager.Setup();
        SceneManager.Setup();
    }
}