
using System.Collections.Generic;

public class ProductionModule : LogicModule
{
    public override IResult Calculate(Data data)
    {
        var resourceProdResult = new ProductionResult();

        var polys = data.Planet.Polygons.Entities;
        foreach (var poly in polys)
        {
            var buildings = poly.GetBuildings(data);
            if (buildings == null) continue;
            foreach (var building in buildings)
            {
                if (building.Model.Model() is ResourceProdBuilding rb)
                {
                    rb.Produce(resourceProdResult, building, 1f, data);
                }
            }
        }
        return resourceProdResult;
    }
}
