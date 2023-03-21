using Godot;
using System;
using System.Collections.Generic;

public class GenData : Data
{
    public LandSeaManager LandSea { get; private set; }
    public GenAuxiliaryData GenAuxData { get; private set; }
    public GeneratorEvents Events { get; private set; }
    public MapGenInfo GenInfo { get; set; }
    public GenerationMultiSettings GenMultiSettings { get; private set; }
    public GenData(GenerationMultiSettings genMultiSettings)
    {
        GenMultiSettings = genMultiSettings;
    }

    protected override void Init()
    {
        GenAuxData = new GenAuxiliaryData(this);
        LandSea = new LandSeaManager();
        Events = new GeneratorEvents();
        base.Init();
    }
    public void ClearAuxData()
    {
        GenAuxData = null;
    }
}
