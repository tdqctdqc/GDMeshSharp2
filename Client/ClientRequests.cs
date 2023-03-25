using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ClientRequests
{
    public RefAction<string> OpenWindowRequest { get; private set; }

    public ClientRequests()
    {
        OpenWindowRequest = new RefAction<string>();
    }
    public void OpenWindow<T>(string name) where T : WindowDialog
    {
        OpenWindowRequest?.Invoke(name);
    }
}
