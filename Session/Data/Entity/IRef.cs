using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IRef
{
    void SyncRef(Data data);

}

public interface IRef<TUnderlying> : IRef
{
}

public class RefAttribute : Attribute
{
    
}

