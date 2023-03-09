
using System.Collections.Generic;

public interface IChain<T>
{
    IReadOnlyList<T> Elements { get; }
}



