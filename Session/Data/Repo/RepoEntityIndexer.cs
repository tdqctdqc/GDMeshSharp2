
using System;

public class RepoEntityIndexer<TEntity, TKey> : RepoIndexer<TEntity, int>
    where TEntity : Entity where TKey : Entity
{
    public TEntity this[TKey k] => this[k.Id]; 
    public RepoEntityIndexer(Data data, Func<TEntity, TKey> get) 
        : base(data, e => get(e).Id)
    {
        
    }
}
