using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonExt
{
    public static bool PointInPoly(this MapPolygon poly, Vector2 posAbs, Data data)
    {
        return data.Cache.PolyRelWheelTris[poly].Any(t => t.ContainsPoint(poly.GetOffsetTo(posAbs, data)));
    }
    public static Vector2 GetOffsetTo(this MapPolygon poly, MapPolygon p, Data data)
    {
        var off1 = p.Center - poly.Center;
        var off2 = (off1 + Vector2.Right * data.Planet.Width);
        var off3 = (off1 + Vector2.Left * data.Planet.Width);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    public static Vector2 GetOffsetTo(this MapPolygon poly, Vector2 p, Data data)
    {
        var off1 = p - poly.Center;
        var off2 = (off1 + Vector2.Right * data.Planet.Width);
        var off3 = (off1 + Vector2.Left * data.Planet.Width);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    public static int GetNumPeeps(this MapPolygon poly, Data data)
    {
        if (data.Planet.Polygons.PeepsInPoly[poly] is HashSet<Peep> h)
        {
            return h.Count;
        }
        return 0;
    }
    public static float GetArea(this MapPolygon poly, Data data)
    {
        return data.Cache.PolyRelWheelTris[poly].Sum(t => t.GetArea());
    }
    public static float GetScore(this MapPolygon poly, MapPolygon closest, MapPolygon secondClosest, 
        Vector2 pRel, Data data, Func<MapPolygon, float> getScore)
    {
        var l = pRel.Length();
        var closeL = (poly.GetOffsetTo(closest, data) - pRel).Length();
        var secondCloseL = (poly.GetOffsetTo(secondClosest, data) - pRel).Length();
        var totalDist = l + closeL + secondCloseL;
        var closeInt = .5f * Mathf.Lerp(getScore(poly), getScore(closest), closeL / (l + closeL));
        var secondInt = .5f * Mathf.Lerp(getScore(poly), getScore(secondClosest), secondCloseL / (l + secondCloseL));

        return closeInt + secondInt;
    }
    public static bool HasNeighbor(this MapPolygon poly, MapPolygon n) => poly.Neighbors.Refs().Contains(n);
    public static bool IsLand(this MapPolygon poly) => poly.Altitude > GeologyGenerator.SeaLevel;
    public static bool IsWater(this MapPolygon poly) => IsLand(poly) == false;
    public static bool IsCoast(this MapPolygon poly) => IsLand(poly) && poly.Neighbors.Refs().Any(n => n.IsWater());
    public static MapPolygonEdge GetEdge(this MapPolygon poly, MapPolygon neighbor, Data data) 
        => data.Planet.PolyEdges.GetEdge(poly, neighbor);
    public static PolyBorderChain GetBorder(this MapPolygon poly, MapPolygon neighbor) => poly.NeighborBorders[neighbor.Id];
    public static IEnumerable<PolyBorderChain> GetPolyBorders(this MapPolygon poly) => poly.Neighbors.Refs()
        .Select(n => poly.GetBorder(n));
    public static IChain<LineSegment> GetOrderedNeighborSegments(this MapPolygon poly, Data data)
    {
        var segs = poly.GetOrderedNeighborBorders(data).SelectMany(b => b.Segments).ToList();
        return new Chain<LineSegment, Vector2>(segs);
    }
    public static List<PolyBorderChain> GetOrderedNeighborBorders(this MapPolygon poly, Data data)
    {
        return data.Cache.OrderedNeighborBorders[poly];
    }
    public static List<LineSegment> GetOrderedBoundarySegs(this MapPolygon poly, Data data)
    {
        return data.Cache.OrderedBoundarySegs[poly];
    }

    public static ReadOnlyHash<ResourceDeposit> GetResourceDeposits(this MapPolygon p, Data data)
    {
        var rd = data.Planet.ResourceDeposits.ByPoly[p];
        return rd == null ? null : rd.ReadOnly();
    }

    public static float GetFertility(this MapPolygon poly)
    {
        return poly.TerrainTris.Tris.Count() > 0 
            ? poly.TerrainTris.Tris.Select(i => i.GetFertility()).Sum()
            : 0f;
    }

    public static IEnumerable<Building> GetBuildings(this MapPolygon poly, Data data)
    {
        return data.Society.Buildings.ByPoly[poly];
    }
}