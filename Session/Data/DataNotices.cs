using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DataNotices
{
    public Action FinishedStateSync { get; set; }
    public Action<Decision> NeedDecision { get; set; }
    //todo danger if entities are derived types? 
    
    public DataNotices()
    {
    }
}

