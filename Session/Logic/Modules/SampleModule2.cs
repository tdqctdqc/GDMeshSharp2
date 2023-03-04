using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class SampleModule2 : LogicModule
{
    public override List<Procedure> Calculate(Data data)
    {
        System.Threading.Thread.Sleep(100);
        
        return new List<Procedure>{ new SampleProc2() };
    }
}