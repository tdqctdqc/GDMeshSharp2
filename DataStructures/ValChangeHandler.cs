using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public abstract class ValChangeHandler
{
    public abstract void Handle(ValChangeNotice n);
    public static ValChangeHandler ConstructFromType(Type propertyType)
    {
        return (ValChangeHandler) typeof(ValChangeHandler<>)
            .MakeGenericType(propertyType)
            .GetMethod(nameof(ValChangeHandler<int>.Construct), BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, null);
    }
}

public class ValChangeHandler<TProperty> : ValChangeHandler
{
    private RefAction<ValChangeNotice<TProperty>> _refAction;
    private Dictionary<int, RefAction<ValChangeNotice<TProperty>>> _specifics;

    public static ValChangeHandler<TProperty> Construct()
    {
        return new ValChangeHandler<TProperty>();
    }
    public ValChangeHandler() : base()
    {
        _refAction = new RefAction<ValChangeNotice<TProperty>>();
        _specifics = new Dictionary<int, RefAction<ValChangeNotice<TProperty>>>();
    }

    public override void Handle(ValChangeNotice n)
    {
        if (n is ValChangeNotice<TProperty> p == false)
        {
            throw new SerializationException($"notice for field {n.FieldName} of type {n.GetType()}" +
                                             $"is not for val type {typeof(TProperty)}");
        }
        _refAction.Invoke(p);
        if(_specifics.TryGetValue(n.Entity.Id, out var specificAction))
        {
            specificAction.Invoke(p);
        }
    }
    public void Subscribe(RefAction<ValChangeNotice<TProperty>> callback)
    {
        _refAction.Subscribe(callback);
    }
    public void SubscribeForSpecific(int entityId, RefAction<ValChangeNotice<TProperty>> callback)
    {
        if (_specifics.ContainsKey(entityId) == false)
        {
            _specifics.Add(entityId, new RefAction<ValChangeNotice<TProperty>>());
        }
        _specifics[entityId].Subscribe(callback);
    }
}
