using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Regime 
{
    public Color PrimaryColor { get; private set; }
    public Color SecondaryColor { get; private set; }
    public RegimeTerritory Territory { get; private set; }

    public Regime(Color primaryColor, Color secondaryColor, GeoPolygon seed)
    {
        PrimaryColor = primaryColor;
        SecondaryColor = secondaryColor;
        Territory = new RegimeTerritory(this);
        Territory.AddSub(seed);
    }
}