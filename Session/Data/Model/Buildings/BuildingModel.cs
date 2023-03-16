
public abstract class BuildingModel : IModel
{
    public string Name { get; }

    public BuildingModel(string name)
    {
        Name = name;
    }

    public abstract bool CanBuild(PolyTri t);
    
}
