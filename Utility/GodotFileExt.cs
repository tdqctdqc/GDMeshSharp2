using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GodotFileExt
{
    public static List<string> GetAllFilesOfType(string path, string type)
    {
        var filePaths = new List<string>();
        var dir = new Directory();
        var e = dir.Open(path);
        if (e != Error.Ok) return filePaths;
        dir.ListDirBegin();
        var filename = dir.GetNext();
        while(filename != "")
        {
            if (dir.CurrentIsDir() && filename.BeginsWith(".") == false)
            {
                filePaths.AddRange(GetAllFilesOfType(path.PlusFile(filename), type));
            }
            else if(filename.EndsWith(type))
            {
                filePaths.Add(path.PlusFile(filename));
            }

            filename = dir.GetNext();
        }

        return filePaths;
    }
    
    
    public static List<string> GetAllFilesOfTypes(string path, List<string> types)
    {
        var filePaths = new List<string>();
        var dir = new Directory();
        var e = dir.Open(path);
        if (e != Error.Ok) return filePaths;
        dir.ListDirBegin();
        var filename = dir.GetNext();
        while(filename != "")
        {
            if (dir.CurrentIsDir() && filename.BeginsWith(".") == false)
            {
                filePaths.AddRange(GetAllFilesOfTypes(path.PlusFile(filename), types));
            }
            else if(types.Any(t => filename.EndsWith(t)))
            {
                filePaths.Add(path.PlusFile(filename));
            }

            filename = dir.GetNext();
        }

        return filePaths;
    }
}
