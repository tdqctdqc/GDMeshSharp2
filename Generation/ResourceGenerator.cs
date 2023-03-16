
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ResourceGenerator : Generator
{
    private GenData _data;
    private GenWriteKey _key;
    public override GenReport Generate(GenWriteKey key)
    {
        _data = key.GenData;
        _key = key;
        var report = new GenReport(GetType().Name);
        GenerateResources();
        return report;
    }

    private void GenerateResources()
    {
        var deposits = new ConcurrentDictionary<NaturalResource, Dictionary<MapPolygon, float>>();
        var resources = _data.Models.Resources.Models.Values;
        Parallel.ForEach(resources, r =>
        {
            if (r is NaturalResource n == false) return;
            deposits.TryAdd(n, n.GenerateDeposits(_data));
        });
        foreach (var kvp in deposits)
        {
            var resource = kvp.Key;
            var rDeposits = kvp.Value;
            foreach (var kvpInner in rDeposits) ResourceDeposit.Create(resource, kvpInner.Key, kvpInner.Value, _key);
        }
    }
}