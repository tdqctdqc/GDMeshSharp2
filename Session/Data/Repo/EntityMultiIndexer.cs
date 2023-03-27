
using System;
using System.Collections.Generic;
using System.Linq;

public class EntityMultiIndexer<TSingle, TMult> : AuxData<TMult>
    where TSingle : Entity where TMult : Entity
{
    public HashSet<TMult> this[TSingle s] => _dic.ContainsKey(s.Id) 
        ? _dic[s.Id].Select(i => Game.I.RefFulfiller.Get<TMult>(i)).ToHashSet() 
        : null;
    protected Dictionary<int, HashSet<int>> _dic;
    private Func<TMult, EntityRef<TSingle>> _getSingle;

    public EntityMultiIndexer(Data data, Func<TMult, EntityRef<TSingle>> getSingle,
        string fieldNameOnMult) : base(data)
    {
        _dic = new Dictionary<int, HashSet<int>>();
        _getSingle = getSingle;
        data.SubscribeForValueChange<TMult, EntityRef<TSingle>>(
            fieldNameOnMult,
            n => 
            {
                if (_dic.TryGetValue(n.OldVal.RefId, out var hash))
                {
                    hash.Remove(n.Entity.Id);
                }
                _dic.AddOrUpdate(n.NewVal.RefId, n.Entity.Id);
            }
        );
        data.SubscribeForDestruction<TSingle>(HandleTSingleRemoved);
    }
    
    public override void HandleAdded(TMult added)
    {
        var single = _getSingle(added);
        if(single != null) _dic.AddOrUpdate(single.RefId, added.Id);
    }

    private void HandleTSingleRemoved(EntityDestroyedNotice n)
    {
        _dic.Remove(n.Entity.Id);
    }
    public override void HandleRemoved(TMult removing)
    {
        var single = _getSingle(removing);
        if(single != null) _dic[single.RefId].Remove(removing.Id);
    }
}
