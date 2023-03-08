
using System.Collections.Generic;

public interface IBorder<T>
{
    IReadOnlyList<T> Elements { get; }
}
