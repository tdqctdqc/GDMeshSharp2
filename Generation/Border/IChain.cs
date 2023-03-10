
using System.Collections.Generic;

public interface IChain<TSegment, TPrim> : ISegment<TPrim> where TSegment : ISegment<TPrim>
{
    IReadOnlyList<TSegment> Segments { get; }
}

public interface IChain<TPrim> : IChain<Segment<TPrim>, TPrim>
{
    
}


