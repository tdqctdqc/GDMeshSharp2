using System;
using System.Collections.Generic;
using System.Linq;

public abstract class EntityTypeValChangeHandler
{
    public string FieldName { get; private set; }
    public abstract void Invoke(ValChangeNotice<> n);
}
public class EntityTypeValChangeHandler<TProperty> : EntityTypeValChangeHandler
{
    
}
