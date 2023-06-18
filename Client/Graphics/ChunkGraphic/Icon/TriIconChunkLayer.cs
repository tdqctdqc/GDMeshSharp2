using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TriIconChunkLayer : MapChunkGraphicLayer
{
    protected abstract IEnumerable<TriIcon> GetIcons(MapPolygon poly, Data data);
    public TriIconChunkLayer(MapChunk chunk, Data data, ChunkChangeListener listener) 
        : base(chunk, listener)
    {
        Draw(data);
    }

    protected TriIconChunkLayer()
    {
    }

    public override void Draw(Data data)
    {
        this.ClearChildren();
        
        foreach (var p in Chunk.Polys)
        {
            var offset = Chunk.RelTo.GetOffsetTo(p, data);
            var icons = GetIcons(p, data);
            if (icons == null) continue;
            foreach (var icon in icons)
            {
                var mesh = icon.Icon.GetMeshInstance();
                var pos = offset + icon.Pos.Tri(data).GetCentroid();
                mesh.Position = pos;
                AddChild(mesh);
            }
        }
    }

    protected class TriIcon
    {
        public Icon Icon { get; private set; }
        public PolyTriPosition Pos { get; private set; }

        public TriIcon(Icon icon, PolyTriPosition pos)
        {
            Icon = icon;
            Pos = pos;
        }
    }
}
