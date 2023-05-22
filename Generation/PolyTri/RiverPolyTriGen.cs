using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RiverPolyTriGen
{
    public void DoRivers(GenWriteKey key)
    {
        var rd = TempRiverData.Construct(key);

        key.Data.Planet.PolygonAux.LandSea.Landmasses.ForEach(lm => DoLandmass(lm, key));
        key.Data.Notices.SetPolyShapes?.Invoke();
        rd.GenerateInfos(key);
        key.Data.Notices.SetPolyShapes?.Invoke();
    }
    private void DoLandmass(HashSet<MapPolygon> lm, GenWriteKey key)
    {
        var riverNexi = key.Data.Planet.PolyNexi.Entities
            .Where(n => n.IncidentPolys.Any(p => lm.Contains(p)))
            .Where(n => n.IncidentEdges.Any(e => e.IsRiver()));
        var riverPolys = riverNexi.SelectMany(n => n.IncidentPolys).Distinct();
        MakeInners(riverNexi, key);
        MakePivots(riverNexi, key);
    }

    private void MakeInners(IEnumerable<MapPolyNexus> rNexi, GenWriteKey key)
    {
        var rd = key.Data.Planet.GetRegister<TempRiverData>().Entities.First();
 
        foreach (var nexus in rNexi)
        {
            if(nexus.IsRiverNexus() == false && nexus.IncidentPolys.Any(p => p.IsWater()) == false) continue;
            // var width = rEdges.Select(re => River.GetWidthFromFlow(re.MoistureFlow)).Average();
            var rEdges = nexus.IncidentEdges.Where(e => e.IsRiver());

            foreach (var poly in nexus.IncidentPolys)
            {
                var nexusRelative = poly.GetOffsetTo(nexus.Point, key.Data);
                // if (nexus.IncidentEdges.Count() > 3) throw new Exception();
                var polyNexusEdges = nexus.IncidentEdges.Where(e => e.EdgeToPoly(poly));
                
                if (polyNexusEdges.Count() == 2)
                {
                    var e1 = polyNexusEdges.ElementAt(0);
                    var e2 = polyNexusEdges.ElementAt(1);
                    
                    float width1;
                    if(e1.IsRiver()) width1 = River.GetWidthFromFlow(e1.MoistureFlow);
                    else if (e2.IsRiver()) width1 = River.GetWidthFromFlow(e2.MoistureFlow);
                    else
                    {
                        width1 = nexus.IncidentEdges.Average(e => River.GetWidthFromFlow(e.MoistureFlow));
                    }
                    var e1segs = e1.GetSegsRel(poly).Segments;
                    var nexusSeg1 = e1segs.First(s => s.From == nexusRelative || s.To == nexusRelative);
                    var axis1 = nexusSeg1.To - nexusSeg1.From;
                    var shift1 = axis1.Rotated(-Mathf.Pi / 2f).Normalized() * width1 / 2f;
                    
                    float width2; 
                    if(e2.IsRiver()) width2 = River.GetWidthFromFlow(e2.MoistureFlow);
                    else if (e1.IsRiver()) width2 = width1;
                    else
                    {
                        width2 = nexus.IncidentEdges.Average(e => River.GetWidthFromFlow(e.MoistureFlow));
                    }
                    var e2segs = e2.GetSegsRel(poly).Segments;
                    var nexusSeg2 = e2segs.First(s => s.From == nexusRelative || s.To == nexusRelative);
                    var axis2 = nexusSeg2.To - nexusSeg2.From;
                    var shift2 = axis2.Rotated(-Mathf.Pi / 2f).Normalized() * width2 / 2f;
                    

                    var intersect = Vector2Ext.GetLineIntersection(nexusSeg1.From - shift1, nexusSeg1.To - shift1, 
                        nexusSeg2.From - shift2, nexusSeg2.To - shift2, 
                        out var inner);
                    if (intersect)
                    {
                        rd.Inners.Add(new PolyCornerKey(nexus, poly), inner);
                    }
                    else throw new Exception();
                }
                else if (polyNexusEdges.Count() == 1)
                {
                    var edge = polyNexusEdges.First();
                    var width = River.GetWidthFromFlow(edge.MoistureFlow);
                    var edgeSegs = edge.GetSegsRel(poly).Segments;
                    var edgeFrom = edgeSegs[0].From;
                    var edgeTo = edgeSegs[edgeSegs.Count - 1].To;
                    var edgeAxis = (edgeTo - edgeFrom).Normalized();
                    
                    Vector2 nexusEdgeAxis;
                    if (nexusRelative == edgeFrom)
                    {
                        nexusEdgeAxis = (edgeSegs[0].To - nexusRelative).Normalized();
                    }
                    else if (nexusRelative == edgeTo)
                    {
                        nexusEdgeAxis = (nexusRelative - edgeSegs.Last().From).Normalized();
                    }
                    else throw new Exception();

                    var axisPerp = nexusEdgeAxis.Rotated(-Mathf.Pi / 2f);

                    var intersect = Vector2Ext.GetLineIntersection(nexusRelative, Vector2.Zero,
                        nexusRelative + axisPerp * width / 2f, nexusRelative + nexusEdgeAxis + axisPerp * width / 2f,
                        out var inner);
                    if (intersect == false) throw new Exception();
                    
                    rd.Inners.Add(new PolyCornerKey(nexus, poly), inner);
                }
                else throw new Exception();
            }
        }
    }

    private void MakePivots(IEnumerable<MapPolyNexus> rNexi, GenWriteKey key)
    {
        var rd = key.Data.Planet.GetRegister<TempRiverData>().Entities.First();
        foreach (var nexus in rNexi)
        {
            var edges = nexus.IncidentEdges;
            foreach (var edge in edges)
            {
                var cornerKey = new EdgeEndKey(nexus, edge);
                var k = new PolyCornerKey(nexus, edge.HighPoly.Entity());
                var hiPolyInner = rd.Inners[k];
                var loPolyInner = rd.Inners[new PolyCornerKey(nexus, edge.LowPoly.Entity())];
                var loInnerRelToHi = loPolyInner + edge.HighPoly.Entity().GetOffsetTo(edge.LowPoly.Entity(), key.Data);
                var pivot = (hiPolyInner + loInnerRelToHi) / 2f;
                
                rd.HiPivots.Add(new EdgeEndKey(nexus, edge), pivot);

                var oldHiSegs = edge.HighSegsRel().Segments.Select(s => new LineSegment(s.From, s.To)).ToList();
                var points = edge.HighSegsRel().Segments.GetPoints().ToList();
                var from = points[0];
                var to = points[points.Count - 1];
                var axis = to - from;
                var edgeLength = from.DistanceTo(to);
                points.Add(pivot);
                points = points.Where(p =>
                {
                    var arm = p - from;
                    var projLength = arm.GetProjectionLength(axis);
                    if (projLength <= 0 && p != from) return false;
                    if (projLength >= edgeLength && p != to) return false;
                    return true;
                }).OrderBy(p =>
                {
                    var arm = p - from;
                    return arm.GetProjectionLength(axis);
                }).ToList();
                if (points[0] != from) throw new Exception();   
                if (points[points.Count - 1] != to) throw new Exception();
                
                var newHiSegs = new List<LineSegment>();
                for (var i = 0; i < points.Count - 1; i++)
                {
                    if (points[i] == points[i + 1]) continue;
                    newHiSegs.Add(new LineSegment(points[i], points[i + 1]));
                }
                
                var newAbsSegs = newHiSegs.Select(s => s.Translate(edge.HighPoly.Entity().Center))
                    .Where(ls => ls.From != ls.To)
                    .ToList();
            }
        }
    }
}