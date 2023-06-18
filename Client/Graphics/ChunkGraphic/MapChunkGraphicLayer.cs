using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class MapChunkGraphicLayer : Node2D
{
    public MapChunk Chunk { get; private set; }
    private ChunkChangeListener _listener;

    public MapChunkGraphicLayer(MapChunk chunk, ChunkChangeListener listener)
    {
        Chunk = chunk;
        _listener = listener;
    }
    protected MapChunkGraphicLayer()
    {
        
    }

    public void Update(Data data)
    {
        if (_listener == null) return;
        if (_listener.Changed.Contains(Chunk))
        {
            Draw(data);
        }
    }
    public abstract void Draw(Data data);
}
