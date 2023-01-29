using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CommandWrapper
{
    public string CommandName { get; private set; }
    public byte[] CommandBytes { get; private set; }

    public CommandWrapper(string commandName, byte[] commandBytes)
    {
        CommandName = commandName;
        CommandBytes = commandBytes;
    }
}