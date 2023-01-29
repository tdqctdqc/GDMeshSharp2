using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UpdateWrapper
{
    public string UpdateName { get; private set; }
    public byte[] UpdateBytes { get; private set; }

    public UpdateWrapper(string updateName, byte[] updateBytes)
    {
        UpdateName = updateName;
        UpdateBytes = updateBytes;
    }
}