
using System;
using System.Collections.Generic;
using System.Linq;

public interface IRegion<TElement>
{
    IReadOnlyHash<TElement> Elements { get; }
}

public interface IRegion<TElement, TEdge>
{
    IReadOnlyGraph<TElement, TEdge> Graph { get; } 
}

public static class IRegionExt
{
    
    
}
