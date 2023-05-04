using System;
using System.Collections.Generic;
using System.Linq;

public class PeepJobAttribute : GameAttribute
{
    public static FarmerAttribute FarmerAttribute { get; private set; } = new FarmerAttribute();
    public static ProleAttribute ProleAttribute { get; private set; } = new ProleAttribute();
    public static BureaucratAttribute BureaucratAttribute { get; private set; } = new BureaucratAttribute();
    public static MinerAttribute MinerAttribute { get; private set; } = new MinerAttribute();
    public static ConstructionAttribute ConstructionAttribute { get; private set; } = new ConstructionAttribute();

    public PeepClass PeepClass { get; protected set; }
    protected PeepJobAttribute(PeepClass peepClass)
    {
        PeepClass = peepClass;
    }
}
