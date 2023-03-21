using System;
using System.Collections.Generic;
using System.Linq;

public class SocietySettings : Settings
{
    public FloatSettingsOption RoadBuildDist { get; private set; }
        = new FloatSettingsOption("RoadBuildDist", 1000f, 100f, 10000f, 10f, false);
    public FloatSettingsOption FertilityPerFarm { get; private set; }
        = new FloatSettingsOption("FertilityPerFarm", 15f, 5f, 50f, 1f, false);
    public FloatSettingsOption FertilityToGetOneFarm { get; private set; }
        = new FloatSettingsOption("FertilityToGetOneFarm", 7f, 2f, 25f, 1f, false);
    public SocietySettings() : base("Society")
    {
        _options.AddRange(new ISettingsOption[]
        {
            RoadBuildDist, FertilityPerFarm, FertilityToGetOneFarm
        });
    }
}
