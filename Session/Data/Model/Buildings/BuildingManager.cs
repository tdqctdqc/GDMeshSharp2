using System.Collections.Generic;

public class BuildingManager : IModelManager<BuildingModel>
{
    public Dictionary<string, BuildingModel> Models { get; private set; }
    
    private void AddJobs(params BuildingModel[] models)
    {
        foreach (var model in models)
        {
            Models.Add(model.Name, model);
        }
    }
}
