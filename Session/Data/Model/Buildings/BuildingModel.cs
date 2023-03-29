
using Godot;

public abstract class BuildingModel : IModel
{
    public string Name { get; }
    public float BuildCost { get; private set; }
    public Icon BuildingIcon { get; }
    public BuildingModel(string name, float buildCost)
    {
        Name = name;
        BuildCost = buildCost;
        BuildingIcon = Icon.Create(Name, Icon.AspectRatio._1x1, 20f);
    }

    public abstract bool CanBuildInTri(PolyTri t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);
    
}
