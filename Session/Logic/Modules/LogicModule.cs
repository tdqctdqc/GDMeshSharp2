using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class LogicModule
{
    public abstract void Calculate(Data data, Action<Message> queue);
}