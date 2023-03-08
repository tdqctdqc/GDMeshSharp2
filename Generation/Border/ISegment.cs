
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ISegment
{
    object From { get; }
    object To { get; }
    ISegment ReverseGeneric();
}
public interface ISegment<T> : ISegment
{
    T From { get; }
    T To { get; }
    ISegment<T> ReverseGeneric();
}

