using System;
using System.Collections.Generic;
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
}
