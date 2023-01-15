using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct BorderEdge<TNode>
{
    public TNode Native { get; set; }
    public TNode Foreign { get; set; }

    public BorderEdge(TNode native, TNode foreign)
    {
        Native = native;
        Foreign = foreign;
    }
}