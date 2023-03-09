
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ISegment
{
    bool PointsTo(ISegment s);
    bool ComesFrom(ISegment s);
}
public interface ISegment<T> : ISegment
{
    T From { get; }
    T To { get; }
    ISegment<T> ReverseGeneric();
}

