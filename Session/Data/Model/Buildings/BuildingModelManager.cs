using System.Collections.Generic;

public class BuildingModelManager : IModelManager<BuildingModel>
{
    public Dictionary<string, BuildingModel> Models { get; private set; }
    public static BuildingModel Farm { get; private set; } = new Farm();
    
    public BuildingModelManager()
    {
        Models = new Dictionary<string, BuildingModel>();
        AddBuildings(Farm);
    }
    private void AddBuildings(params BuildingModel[] models)
    {
        foreach (var model in models)
        {
            Models.Add(model.Name, model);
        }
    }
}
