using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepHomeAuxData : RepoAuxData<Peep>
{
    private Dictionary<MapPolygon, HashSet<Peep>> _polyPeeps;
    
    public PeepHomeAuxData(Repository<Peep> repo) : base(repo.Domain.Data)
    {
        _polyPeeps = new Dictionary<MapPolygon, HashSet<Peep>>();
        
        repo.RegisterForValueChangeCallback<EntityRef<MapPolygon>>(nameof(Peep.Home),
        n =>
        {
            RemovePeepFromPoly(n.Entity, n.OldVal.Entity());
            AddPeepToPoly(n.Entity, n.NewVal.Entity());
        });
    }

    public int GetNumPeepsInPoly(MapPolygon poly)
    {
        if (_polyPeeps.ContainsKey(poly)) return _polyPeeps[poly].Count;
        return 0;
    }
    public override void HandleAdded(Peep added)
    {
        AddPeepToPoly(added, added.Home.Entity());
    }
    public override void HandleRemoved(Peep removing)
    {
        RemovePeepFromPoly(removing, removing.Home.Entity());
    }
    private void AddPeepToPoly(Peep added, MapPolygon poly)
    {
        if(_polyPeeps.ContainsKey(poly) == false)
            _polyPeeps.Add(poly, new HashSet<Peep>());
        _polyPeeps[poly].Add(added);
    }
    

    private void RemovePeepFromPoly(Peep toRemove, MapPolygon poly)
    {
        if (_polyPeeps.ContainsKey(poly) == false)
            return;
        _polyPeeps[poly].Remove(toRemove);
        if (_polyPeeps[poly].Count == 0) _polyPeeps.Remove(poly);
    }
}