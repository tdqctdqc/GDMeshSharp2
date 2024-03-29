using System;
using System.Collections.Generic;
using System.Linq;

public class RegimeFinance
{
    public int FinancialPower { get; private set; }

    public static RegimeFinance Construct()
    {
        return new RegimeFinance(0);
    }
    private RegimeFinance(int financialPower)
    {
        FinancialPower = financialPower;
    }

    public int GetIncome(Regime r, Data d)
    {
        var fromBuildings = r.Polygons
            .Where(p => p.GetBuildings(d) != null)
            .Sum(p =>
                {
                    var bs = p.GetBuildings(d);
                        if(bs == null) return 0;

                    return bs.Select(b => b.Model.Model())
                        .SelectWhereOfType<BuildingModel, WorkBuildingModel>()
                        .Sum(b => b.Income);
                }
            );
        var fromAgriculture = r.Polygons.Sum(p => p.PolyFoodProd.Income(d));
        
        return fromBuildings + fromAgriculture;
    }
}
