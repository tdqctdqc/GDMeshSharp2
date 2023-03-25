using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ClientRequests
{
    public Action<Type> OpenWindowType { get; set; }
    public void OpenWindow<T>() where T : WindowDialog
    {
        OpenWindowType?.Invoke(typeof(T));
    }
}
