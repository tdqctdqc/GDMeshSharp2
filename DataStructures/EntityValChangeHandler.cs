using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EntityValChangeHandler
{
    private Dictionary<string, ValChangeHandler> _valHandlers;
    private IEntityMeta _meta;
    public EntityValChangeHandler(Type entityType)
    {
        _meta = Game.I.Serializer.GetEntityMeta(entityType);
        _valHandlers = new Dictionary<string, ValChangeHandler>();
        foreach (var field in _meta.FieldTypes)
        {
            _valHandlers.Add(field.Key, ValChangeHandler.ConstructFromType(field.Value));
        }
    }
    public void HandleChange(ValChangeNotice notice)
    {
        if(_valHandlers.TryGetValue(notice.FieldName, out var r))
        {
            r.Handle(notice);
        }
    }
    public void Subscribe(string fieldName, Action<ValChangeNotice> callback)
    {
        if(_valHandlers.ContainsKey(fieldName) == false)
        {
            if (_meta.FieldNameHash.Contains(fieldName) == false)
            {
                foreach (var s in _meta.FieldNameList)
                {
                    GD.Print(s);
                }
                throw new NoticeException($"field {fieldName} not found for {_meta.EntityType}");
            }
            _valHandlers.Add(fieldName, ValChangeHandler.ConstructFromType(_meta.FieldTypes[fieldName]));
        }
        var handler = _valHandlers[fieldName];
        handler.Subscribe(callback);
    }
    public void Subscribe<TProperty>(string fieldName, Action<ValChangeNotice<TProperty>> callback)
    {
        if(_valHandlers.ContainsKey(fieldName) == false)
        {
            if (_meta.FieldNameHash.Contains(fieldName) == false)
            {
                foreach (var s in _meta.FieldNameList)
                {
                    GD.Print(s);
                }
                throw new NoticeException($"field {fieldName} not found for {_meta.EntityType}");
            }
            _valHandlers.Add(fieldName, new ValChangeHandler<TProperty>());
        }

        var propType = _meta.FieldTypes[fieldName];
        if (typeof(TProperty).IsAssignableFrom(propType) == false)
        {
            throw new NoticeException($"{fieldName} type is {propType} not assignable to {typeof(TProperty)}");
        }

        var h = _valHandlers[fieldName];
        if (h is ValChangeHandler<TProperty> == false)
        {
            GD.Print(h.GetType());
            GD.Print(typeof(ValChangeHandler<TProperty>));
        }
        var handler = (ValChangeHandler<TProperty>) _valHandlers[fieldName];
        handler.Subscribe(callback);
    }
    public void SubscribeForSpecific<TProperty>(int entityId, string fieldName, Action<ValChangeNotice<TProperty>> callback)
    {
        if(_valHandlers.ContainsKey(fieldName) == false)
        {
            //todo make hash
            if (_meta.FieldNameList.Contains(fieldName) == false)
            {
                throw new SerializationException($"No field named {fieldName} for {_meta.EntityType}");
            }
            _valHandlers.Add(fieldName, new ValChangeHandler<TProperty>());
        }
        var valHandler = (ValChangeHandler<TProperty>) _valHandlers[fieldName];
        valHandler.SubscribeForSpecific(entityId, n => callback((ValChangeNotice<TProperty>)n));
    }
}
