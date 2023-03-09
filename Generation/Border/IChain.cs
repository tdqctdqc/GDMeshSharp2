
using System.Collections.Generic;

public interface IChain<TSegment> : ISegment
{
    IReadOnlyList<TSegment> Elements { get; }
}



