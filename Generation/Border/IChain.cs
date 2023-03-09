
using System.Collections.Generic;

public interface IChain<TSeg>
{
    IReadOnlyList<TSeg> Elements { get; }
}



