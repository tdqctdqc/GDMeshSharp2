using System;
using System.Collections.Generic;
using System.Linq;
using gdMeshSharp2.Client.Settings;
using Godot;

public class GenerationSettings : ISettings
{
    public IReadOnlyList<ISettingsOption> Options { get; }
    public FloatSettingsOption MapWidth { get; private set; }
        = new FloatSettingsOption("Map Width", 16000f, 4000f, 32000f, 1000f, true);
    public FloatSettingsOption MapHeight { get; private set; }
        = new FloatSettingsOption("Map Height", 8000f, 2000f, 16000f, 1000f, true);
    public FloatSettingsOption Seed { get; private set; }
        = new FloatSettingsOption("Seed", 0f, 0f, 1000f, 1f, true);
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
    public FloatSettingsOption EquatorDistMoistureMultWeight { get; private set; }
        = new FloatSettingsOption("Equator Dist Moisture Mult Weight", .5f, 0f, 1f, .1f, false);
    public FloatSettingsOption RiverFlowPerMoisture { get; private set; }
        = new FloatSettingsOption("River Flow Per Moisture", 10f, 0f, 50f, 1f, false);
    public FloatSettingsOption BaseRiverFlowCost { get; private set; }
        = new FloatSettingsOption("Base River Flow Cost", 100f, 0f, 1000f, 10f, false);
    public FloatSettingsOption RiverFlowCostRoughnessMult { get; private set; }
        = new FloatSettingsOption("River Flow Cost Roughness Mult", 1f, 0f, 10f, 1f, false);
    
    public FloatSettingsOption PreferredMinPolyEdgeLength { get; private set; }
        = new FloatSettingsOption("PreferredMinPolyEdgeLength", 50f, 10f, 100f, 1f, false);
    public Vector2 Dimensions => new Vector2(MapWidth.Value, MapHeight.Value);
    public GenerationSettings()
    {
        Options = new List<ISettingsOption>
        {
            Seed, MapWidth, MapHeight, SeaLevel, FaultLineRange, FrictionAltEffect,
            FrictionRoughnessEffect, RoughnessErosionMult, EquatorDistMoistureMultWeight,
            RiverFlowPerMoisture, BaseRiverFlowCost, RiverFlowCostRoughnessMult,
            PreferredMinPolyEdgeLength
        };
    }
    
}
