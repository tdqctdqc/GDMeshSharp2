
using Godot;

public abstract class BuildingModel : IModel
{
    public string Name { get; }
    public Icon BuildingIcon { get; } 

    public BuildingModel(string name)
    {
        Name = name;
        BuildingIcon = Icon.Create(Name, Icon.AspectRatio.Square);
    }

    public abstract bool CanBuildInTri(PolyTri t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);
    
}
