
using System.Collections.Generic;

public class ProductionModule : LogicModule
{
    public override IResult Calculate(Data data)
    {
        var resourceProdResult = new ProductionResult();
        var buildings = data.Society.Buildings.Entities;
        foreach (var building in buildings)
        {
            if (building.Model.Model() is ResourceProdBuilding rb)
            {
                rb.Produce(resourceProdResult, building, 1f, data);
            }
        }
        return resourceProdResult;
    }
}
