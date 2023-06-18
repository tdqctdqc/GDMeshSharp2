using System;
using System.Collections.Generic;
using System.Linq;

public class BuildingIconLayer : TriIconChunkLayer
{
    public BuildingIconLayer(MapChunk chunk, Data data, MapGraphics mg) 
        : base(chunk, data, mg.ChunkChangedCache.BuildingsChanged)
    {
    }
    private BuildingIconLayer() : base()
    {
    }
    protected override IEnumerable<TriIcon> GetIcons(MapPolygon poly, Data data)
    {
        var buildings = poly.GetBuildings(data);
        if (buildings == null) return null;
        return buildings
            .Select(b => new TriIcon(b.Model.Model().BuildingIcon, b.Position));
    }
}
