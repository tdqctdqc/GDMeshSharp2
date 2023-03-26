using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonExt
{
    public static bool PointInPoly(this MapPolygon poly, Vector2 posAbs, Data data)
    {
        return poly.GetWheelTris(data).Any(t => t.ContainsPoint(poly.GetOffsetTo(posAbs, data)));
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
    public static IEnumerable<Peep> GetPeeps(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.PeepsInPoly[poly];
    }
    public static IReadOnlyList<Triangle> GetWheelTris(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].WheelTris;
    }
    public static float GetArea(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].WheelTris.Sum(t => t.GetArea());
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
    public static bool HasNeighbor(this MapPolygon poly, MapPolygon n) => poly.Neighbors.Entities().Contains(n);
    public static bool IsWater(this MapPolygon poly) => poly.IsLand == false;
    public static bool IsCoast(this MapPolygon poly) => poly.IsLand && poly.Neighbors.Entities().Any(n => n.IsWater());
    public static MapPolygonEdge GetEdge(this MapPolygon poly, MapPolygon neighbor, Data data) 
        => data.Planet.PolyEdgeAux.GetEdge(poly, neighbor);
    public static PolyBorderChain GetBorder(this MapPolygon poly, int nId) => poly.NeighborBorders[nId];
    public static IEnumerable<PolyBorderChain> GetPolyBorders(this MapPolygon poly) => poly.Neighbors.RefIds
        .Select(n => poly.GetBorder(n));
    public static IChain<LineSegment> GetOrderedNeighborSegments(this MapPolygon poly, Data data)
    {
        var segs = poly.GetOrderedNeighborBorders(data).SelectMany(b => b.Segments).ToList();
        return new Chain<LineSegment, Vector2>(segs);
    }
    public static List<PolyBorderChain> GetOrderedNeighborBorders(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].OrderedNeighborBorders;
    }
    public static List<LineSegment> GetOrderedBoundarySegs(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].OrderedBoundarySegs;
    }

    public static ReadOnlyHash<ResourceDeposit> GetResourceDeposits(this MapPolygon p, Data data)
    {
        var rd = data.Planet.ResourceDepositAux.ByPoly[p];
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
        return data.Society.BuildingAux.ByPoly[poly];
    }

    public static Vector2 GetGraphicalCenterOffset(this MapPolygon poly, Data data)
    {
        return data.Planet.PolygonAux.AuxDatas[poly].GraphicalCenter;
    }

    public static bool HasSettlement(this MapPolygon p, Data data)
    {
        return GetSettlement(p, data) != null;
    }
    public static Settlement GetSettlement(this MapPolygon p, Data data)
    {
        return data.Society.SettlementAux.ByPoly.ContainsKey(p) ? data.Society.SettlementAux.ByPoly[p] : null;
    }
}