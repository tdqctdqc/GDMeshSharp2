using System;
using System.Collections.Generic;
using System.Linq;

public class GeologySettings : Settings
{
    public FloatSettingsOption SeaLevel { get; private set; }
        = new FloatSettingsOption("Sea Level", .5f, 0f, 1f, .05f, false);
    public FloatSettingsOption FaultLineRange { get; private set; }
        = new FloatSettingsOption("Fault Line Range", 75f, 0f, 1000f, 1f, false);
    public FloatSettingsOption FrictionAltEffect { get; private set; }
        = new FloatSettingsOption("Friction Alt Effect", .05f, 0f, 1f, .01f, false);
    public FloatSettingsOption FrictionRoughnessEffect { get; private set; }
        = new FloatSettingsOption("Friction Roughness Effect", 1f, 0f, 2f, .1f, false);
    public FloatSettingsOption RoughnessErosionMult { get; private set; }
        = new FloatSettingsOption("Roughness Erosion Multiplier", 1f, 0f, 2f, .1f, false);

    public GeologySettings() : base("Geology")
    {
        _options.AddRange(new ISettingsOption[]
        {
            SeaLevel, FaultLineRange, FrictionAltEffect, FrictionRoughnessEffect, RoughnessErosionMult
        });
    }
}
