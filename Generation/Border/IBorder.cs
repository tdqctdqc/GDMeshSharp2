
using System.Collections.Generic;

public interface IBorder<TSeg, TPrim, TRegion>
{
    TRegion Native { get; }
    TRegion Foreign { get; }
    IReadOnlyList<TSeg> Elements { get; }
}


