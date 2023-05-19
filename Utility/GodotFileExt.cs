using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;

public class GodotFileExt
{
    public static List<string> GetAllFilePathsOfType(string path, string type)
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
                filePaths.AddRange(GetAllFilePathsOfType(path.PlusFile(filename), type));
            }
            else if(filename.EndsWith(type))
            {
                filePaths.Add(path.PlusFile(filename));
            }

            filename = dir.GetNext();
        }

        return filePaths;
    }
    
    
    public static List<string> GetAllFilePathsOfTypes(string path, List<string> types)
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
                filePaths.AddRange(GetAllFilePathsOfTypes(path.PlusFile(filename), types));
            }
            else if(types.Any(t => filename.EndsWith(t)))
            {
                filePaths.Add(path.PlusFile(filename));
            }

            filename = dir.GetNext();
        }

        return filePaths;
    }

    public static string ReadFileAsString(string path)
    {
        var f = new File();
        f.Open(path, File.ModeFlags.Read);
        var sb = new StringBuilder();
        while (f.EofReached() == false)
        {
            sb.Append(f.GetLine());
        }
        return sb.ToString();
    }

    public static string GetFileName(string path)
    {
        var lastSlash = path.LastIndexOf("/");
        var period = path.LastIndexOf(".");
        var length = period - lastSlash - 1;
        return path.Substring(lastSlash + 1, length);
    }
}
