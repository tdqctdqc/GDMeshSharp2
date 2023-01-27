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
        // if(typeof(TRefVar) == typeof(EntityRefCollection<MapPolygon>)) setUnderlyingDel(default, default, default);
        SetUnderlying = (e, ul, k) =>
        {
            GD.Print("setting underlying");
            GD.Print(typeof(TRefVar));
            var p = GetProperty(e);
            if (p == null) throw new Exception();
            setUnderlyingDel(p, ul, k);
            GD.Print("set underlying");
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
    public override object GetFromSerialized(byte[] bytes)
    {
        return Game.I.Serializer.Deserialize<TUnderlying>(bytes);
    }
}