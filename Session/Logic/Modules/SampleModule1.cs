using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class SampleModule1 : LogicModule
{
    public override List<Procedure> Calculate()
    {
        System.Threading.Thread.Sleep(200);
        return new List<Procedure>{ new SampleProc1() };
    }
}