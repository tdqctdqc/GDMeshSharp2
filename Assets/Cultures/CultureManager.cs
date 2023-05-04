using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CultureManager
{
    public static Dictionary<string, Culture> Cultures { get; private set; }
    public static void Setup()
    {
        Cultures = new Dictionary<string, Culture>();
        var culturePaths = GodotFileExt.GetAllFilesOfTypes("res://Assets/Cultures/Cultures/", 
            new List<string>{ ".json"});
        culturePaths.ForEach(s =>
        {
            var culture = new Culture(s);
            Cultures.Add(culture.Name, culture);
        });
        
        foreach (var kvp in Cultures)
        {
            GD.Print(kvp.Key);
        }
    }

}
