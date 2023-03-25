using System.Collections.Generic;
using System.Linq;

public class BuildingModelManager : IModelManager<BuildingModel>
{
    public Dictionary<string, BuildingModel> Models { get; private set; }
    public static Farm Farm { get; private set; } = new Farm();
    public static Mine IronMine => Mines[ItemManager.Iron];
    public static Dictionary<Item, Mine> Mines { get; private set; }
        
    public BuildingModelManager()
    {
        Models = new Dictionary<string, BuildingModel>();
        Mines = new Dictionary<Item, Mine>
        {
            {ItemManager.Iron, new Mine(nameof(Iron) + nameof(Mine), ItemManager.Iron)}
        }; 
        AddBuildings(Mines.Values.ToArray());
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
