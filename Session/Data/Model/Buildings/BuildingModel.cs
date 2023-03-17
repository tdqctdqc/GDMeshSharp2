
using Godot;

public abstract class BuildingModel : IModel
{
    public string Name { get; }
    public abstract Texture Icon { get; }
    public BuildingModel(string name)
    {
        Name = name;
    }

    public abstract bool CanBuildInTri(PolyTri t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);
    
}
