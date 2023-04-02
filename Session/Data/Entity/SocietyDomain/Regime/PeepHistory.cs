using System;
using System.Collections.Generic;
using System.Linq;

public class PeepHistory
{
    public Dictionary<int, int> PeepCountsByTick { get; private set; }
    private List<KeyValuePair<int, int>> _list;
    public static PeepHistory Construct()
    {
        return new PeepHistory(new Dictionary<int, int>());
    }

    private PeepHistory(Dictionary<int, int> peepCountsByTick)
    {
        PeepCountsByTick = peepCountsByTick;
        _list = PeepCountsByTick.ToList();
    }
    public void Update(int tick, int count, ProcedureWriteKey key)
    {
        PeepCountsByTick.Add(tick, count);
        _list.Add(new KeyValuePair<int, int>(tick, count));
    }

    public int GetLatestDelta()
    {
        if (_list.Count > 1)
        {
            return _list[_list.Count - 1].Value - _list[_list.Count - 2].Value;
        }

        return 0;
    }
}
