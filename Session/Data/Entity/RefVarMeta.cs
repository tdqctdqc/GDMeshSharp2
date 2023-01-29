using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class RefVarMeta<TEntity, TRefVar, TUnderlying> : EntityVarMeta<TEntity, TRefVar> where TEntity : Entity
{
    protected Func<TEntity, TUnderlying> GetUnderlying { get; private set; }
    protected Action<TEntity, TUnderlying, StrongWriteKey> SetUnderlying { get; private set; }
    protected Action<TEntity> Initialize { get; private set; }
    public RefVarMeta(PropertyInfo prop) : base(prop)
    {
        
        var getUnderlyingMi = typeof(TRefVar).GetMethod(nameof(IRef<int>.GetUnderlying));
        var getUnderlyingDel = getUnderlyingMi.MakeInstanceMethodDelegate<Func<TRefVar, TUnderlying>>();
        GetUnderlying = e => getUnderlyingDel(GetProperty(e));
        
        var setUnderlyingMi = typeof(TRefVar).GetMethod(nameof(IRef<int>.Set));
        var setUnderlyingDel = setUnderlyingMi.MakeInstanceMethodDelegate<Action<TRefVar, TUnderlying, StrongWriteKey>>();
        SetUnderlying = (e, ul, k) =>
        {
            var p = GetProperty(e);
            setUnderlyingDel(p, ul, k);
        };
    }
    public override void Set(TEntity e, object receivedValue, ServerWriteKey key)
    {
        SetUnderlying(e, (TUnderlying) receivedValue, key);
    }
    public override void Set(TEntity e, object receivedValue, CreateWriteKey key)
    {
        SetUnderlying(e, (TUnderlying) receivedValue, key);
    }
    public override object GetForSerialize(TEntity e)
    {
        return (TUnderlying) GetUnderlying(e);
    }
    public override object GetPropertyFromSerialized(byte[] bytes)
    {
        return Game.I.Serializer.Deserialize<TUnderlying>(bytes);
    }
}