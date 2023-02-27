
using System;
using System.Collections.Generic;
using System.Linq;

public class RepoEntityMultiIndexer<TSingle, TMult> : RepoAuxData<TMult>
    where TSingle : Entity where TMult : Entity
{
    public HashSet<TMult> this[TSingle s] => _dic.ContainsKey(s) 
        ? _dic[s].Select(i => Game.I.RefFulfiller.Get<TMult>(i)).ToHashSet() 
        : null;
    protected Dictionary<TSingle, HashSet<int>> _dic;
    private Func<TMult, TSingle> _getSingle;

    public RepoEntityMultiIndexer(Data data, Func<TMult, TSingle> getSingle,
        string fieldNameOnMult) : base(data)
    {
        _dic = new Dictionary<TSingle, HashSet<int>>();
        _getSingle = getSingle;
        data.Notices.RegisterEntityVarUpdatedCallback<TMult, EntityRef<TSingle>>(
            fieldNameOnMult,
            n => 
            {
                if (_dic.TryGetValue(n.OldVal.Entity(), out var hash))
                {
                    hash.Remove(n.Entity.Id);
                }
                _dic.AddOrUpdate(n.NewVal.Entity(), n.Entity.Id);
            }
        );
        data.Notices.RegisterEntityRemovingCallback<TSingle>(HandleTSingleRemoved);
    }
    
    public override void HandleAdded(TMult added)
    {
        _dic.AddOrUpdate(_getSingle(added), added.Id);
    }

    private void HandleTSingleRemoved(TSingle s)
    {
        _dic.Remove(s);
    }
    public override void HandleRemoved(TMult removing)
    {
        _dic[_getSingle(removing)].Remove(removing.Id);
    }
}
