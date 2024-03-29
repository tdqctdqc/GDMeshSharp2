using System;
using System.Collections.Generic;
using System.Linq;

public class PlanetSettings : Settings
{
    public FloatSettingsOption MapWidth { get; private set; }
        = new FloatSettingsOption("Map Width", 16000f, 4000f, 32000f, 1000f, true);
    public FloatSettingsOption MapHeight { get; private set; }
        = new FloatSettingsOption("Map Height", 8000f, 2000f, 16000f, 1000f, true);
    public FloatSettingsOption Seed { get; private set; }
        = new FloatSettingsOption("Seed", 0f, 0f, 1000f, 1f, true);
    public FloatSettingsOption PreferredMinPolyEdgeLength { get; private set; }
        = new FloatSettingsOption("PreferredMinPolyEdgeLength", 50f, 10f, 100f, 1f, false);
    public PlanetSettings() : base("Planet")
    {
    }
}
