using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkChangeListener
{
    public HashSet<MapChunk> Changed { get; private set; }
    public static ChunkChangeListener ConstructDynamic<TEntity, TValue>(
        Data data, RefAction<ValChangeNotice<TValue>> valueTrigger, Func<TEntity, MapPolygon> getPoly)
        where TEntity : Entity
    {
        var l = new ChunkChangeListener();
        valueTrigger.Subscribe(n =>
        {
            var poly = getPoly((TEntity) n.Entity);
            l.Changed.Add(poly.GetChunk(data));
        });
        return l;
    }
    public static ChunkChangeListener ConstructConstant<TEntity>(Data data, Func<TEntity, MapPolygon> getPoly) 
        where TEntity : Entity
    {
        var l = new ChunkChangeListener();
        data.SubscribeForCreation<TEntity>(n => l.AddedEntity(n, data, getPoly));
        data.SubscribeForDestruction<TEntity>(n => l.RemovedEntity(n, data, getPoly));
        return l;
    }
    public static ChunkChangeListener ConstructConstantMultiple<TEntity>(Data data, Func<TEntity, IEnumerable<MapPolygon>> getPolys) 
        where TEntity : Entity
    {
        var l = new ChunkChangeListener();
        data.SubscribeForCreation<TEntity>(n => l.AddedEntities(n, data, getPolys));
        data.SubscribeForDestruction<TEntity>(n => l.RemovedEntities(n, data, getPolys));
        return l;
    }
    private ChunkChangeListener()
    {
        Changed = new HashSet<MapChunk>();
    }

    public void Clear()
    {
        Changed.Clear();
    }
    private void AddedEntities<TEntity>(EntityCreatedNotice n, Data data, 
        Func<TEntity, IEnumerable<MapPolygon>> getPolys) 
        where TEntity : Entity
    {
        foreach (var e in n.Entities)
        {
            var t = (TEntity) e;
            var polys = getPolys(t);
            foreach (var poly in polys)
            {
                var chunk = poly.GetChunk(data);
                Changed.Add(chunk); 
            }
        }
    }
    private void AddedEntity<TEntity>(EntityCreatedNotice n, Data data, Func<TEntity, MapPolygon> getPoly) 
        where TEntity : Entity
    {
        foreach (var e in n.Entities)
        {
            var t = (TEntity) e;
            var poly = getPoly(t);
            var chunk = poly.GetChunk(data);
            Changed.Add(chunk);
        }
    }
    private void RemovedEntity<TEntity>(EntityDestroyedNotice n, Data data, Func<TEntity, MapPolygon> getPoly) 
        where TEntity : Entity
    {
        var t = (TEntity) n.Entity;
        var poly = getPoly(t);
        var chunk = poly.GetChunk(data);
        Changed.Add(chunk);
    }
    private void RemovedEntities<TEntity>(EntityDestroyedNotice n, Data data, Func<TEntity, IEnumerable<MapPolygon>> getPoly) 
        where TEntity : Entity
    {
        var t = (TEntity) n.Entity;
        var polys = getPoly(t);
        foreach (var poly in polys)
        {
            var chunk = data.Planet.PolygonAux.ChunksByPoly[poly];
            Changed.Add(chunk);
        }
    }
}
