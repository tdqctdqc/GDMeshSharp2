using Godot;
using System;
using System.Collections.Generic;

public class WorldData : Data
{
    public LandSeaManager LandSea { get; private set; }
    public GenAuxiliaryData GenAuxData { get; private set; }
    public WorldData() : base()
    {
        GenAuxData = new GenAuxiliaryData();
        LandSea = new LandSeaManager();
        
    }

    public void ClearAuxData()
    {
        GenAuxData = null;
    }
}
