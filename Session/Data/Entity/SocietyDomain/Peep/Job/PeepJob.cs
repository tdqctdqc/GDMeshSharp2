using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepJob : IModel
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public Icon JobIcon { get; } 
    public AttributeHolder<PeepJobAttribute> Attributes { get; private set; }
    public PeepJob(string name, params PeepJobAttribute[] attributes)
    {
        Name = name;
        JobIcon = Icon.Create(Name, Icon.AspectRatio._1x2, 50f);
        Attributes = new AttributeHolder<PeepJobAttribute>();
        foreach (var att in attributes)
        {
            Attributes.Add(att);
        }
    }
}