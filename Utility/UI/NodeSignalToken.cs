using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class NodeSignalToken<T> : Node
{
    private Action<T> _action;

    public static void Subscribe(Node publisher, string signalName, Action<T> action)
    {
        var token = new NodeSignalToken<T>(action);
        publisher.Connect(signalName, token, nameof(Invoke));
    }
    private NodeSignalToken(Action<T> action)
    {
        _action = action;
    }

    private void Invoke(T val)
    {
        _action.Invoke(val);
    }
}
