
using System.Collections.Generic;
using Godot;

public class TextureManager
{
    public static Dictionary<string, Texture> Textures { get; private set; }
    public static void Setup()
    {
        Textures = new Dictionary<string, Texture>();
        var scenePaths = GodotFileExt.GetAllFilePathsOfTypes("res://Assets/Textures/", 
            new List<string>{ ".png", ".svg" });
        scenePaths.ForEach(path =>
        {
            var text = (Texture) GD.Load(path);
            var textureName = GodotFileExt.GetFileName(path);
            Textures.Add(textureName, text);
        });
    }
}
