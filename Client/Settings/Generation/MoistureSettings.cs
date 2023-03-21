using System;
using System.Collections.Generic;
using System.Linq;

public class MoistureSettings : Settings
{
    public FloatSettingsOption EquatorDistMoistureMultWeight { get; private set; }
        = new FloatSettingsOption("Equator Dist Moisture Mult Weight", .5f, 0f, 1f, .1f, false);
    public FloatSettingsOption RiverFlowPerMoisture { get; private set; }
        = new FloatSettingsOption("River Flow Per Moisture", 10f, 0f, 50f, 1f, false);
    public FloatSettingsOption BaseRiverFlowCost { get; private set; }
        = new FloatSettingsOption("Base River Flow Cost", 100f, 0f, 1000f, 10f, false);
    public FloatSettingsOption RiverFlowCostRoughnessMult { get; private set; }
        = new FloatSettingsOption("River Flow Cost Roughness Mult", 1f, 0f, 10f, 1f, false);

    public MoistureSettings() : base("Moisture")
    {
        _options.AddRange(new ISettingsOption[]
        {
            EquatorDistMoistureMultWeight, RiverFlowPerMoisture, BaseRiverFlowCost, 
            RiverFlowCostRoughnessMult
        });
    }
}
