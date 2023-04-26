using System;
using System.Collections.Generic;
using System.Linq;

public class SocietySettings : Settings
{
    public FloatSettingsOption FertilityPerFarm { get; private set; }
        = new FloatSettingsOption("FertilityPerFarm", 15f, 5f, 50f, 1f, false);
    public FloatSettingsOption FertilityToGetOneFarm { get; private set; }
        = new FloatSettingsOption("FertilityToGetOneFarm", 2f, 2f, 25f, 1f, false);
    public SocietySettings() : base("Society")
    {
    }
}
