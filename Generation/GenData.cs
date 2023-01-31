using Godot;
using System;
using System.Collections.Generic;

public class GenData : Data
{
    public LandSeaManager LandSea { get; private set; }
    public GenAuxiliaryData GenAuxData { get; private set; }
    public GeneratorEvents Events { get; private set; }
    public GenData()
    {
    }

    protected override void Init()
    {
        GenAuxData = new GenAuxiliaryData();
        LandSea = new LandSeaManager();
        Events = new GeneratorEvents();
        base.Init();
    }
    public void ClearAuxData()
    {
        GenAuxData = null;
    }
}