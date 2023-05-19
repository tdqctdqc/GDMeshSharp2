using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyFillLayer : MapChunkGraphicLayer
{
    private Func<MapPolygon, Color> _getColor;
    private float _transparency;
    public PolyFillLayer(MapChunk chunk, Data data, Func<MapPolygon, Color> getColor,
        float transparency = 1f) : base(chunk, null)
    {
        _transparency = transparency;
        _getColor = getColor;
        Draw(data);
    }

    public override void Draw(Data data)
    {
        var mb = new MeshBuilder();
        mb.AddPolysRelative(Chunk.RelTo, Chunk.Polys, _getColor, data);
        var mesh = mb.GetMeshInstance();
        AddChild(mesh);
        Modulate = new Color(Colors.Transparent, _transparency);
    }
}
