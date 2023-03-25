using System;
using System.Collections.Generic;
using System.Linq;

public class AttributeHolder<TAttr> where TAttr : GameAttribute
{
    public Dictionary<Type, TAttr> Attributes { get; private set; }
    public TAttr this[Type type] => Attributes.ContainsKey(type) ? Attributes[type] : null;

    public AttributeHolder()
    {
        Attributes = new Dictionary<Type, TAttr>();
    }
    public void Add(TAttr t)
    {
        Attributes.Add(t.GetType(), t);
    }

    public bool Has<T>()
    {
        return Attributes.ContainsKey(typeof(T));
    }
}
