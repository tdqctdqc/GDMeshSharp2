
using System.Collections.Generic;
using Godot;

public class TextureManager
{
    public static Dictionary<string, Texture> Textures { get; private set; }
    public static void Setup()
    {
        GD.Print("setting up textures");
        Textures = new Dictionary<string, Texture>();
        var scenePaths = GodotFileExt.GetAllFilesOfTypes("res://Assets/Textures/", new List<string>{ ".png", ".svg" });
        scenePaths.ForEach(s =>
        {
            var text = (Texture) GD.Load(s);
            var lastSlash = s.LastIndexOf("/");
            var period = s.LastIndexOf(".");
            var length = period - lastSlash - 1;
            var textureName = s.Substring(lastSlash + 1, length);
            Textures.Add(textureName, text);
        });
    }
}
