
    using System;

    public interface IBoundary<TPrim> : IChain<Segment<TPrim>, TPrim>
    {
        Action<TPrim> CrossedSelf { get; set; }
    }
