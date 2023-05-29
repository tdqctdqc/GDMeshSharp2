using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class RiverPolyTriGen
{
    public TempRiverData DoRivers(GenWriteKey key)
    {
        var rd = new TempRiverData();
        var sw = new Stopwatch();
        
        sw.Start();
        //todo partition by riverpoly union find instead?
        var lms = key.Data.Planet.PolygonAux.LandSea.Landmasses;
        Parallel.ForEach(lms, lm => PreprocessRiversForLandmass(rd, lm, key));
        // lms.ToList().ForEach(lm => PreprocessRiversForLandmass(rd, lm, key));
        sw.Stop();
        GD.Print("preprocessing rivers " + sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        key.Data.Notices.SetPolyShapes?.Invoke();
        GD.Print("set poly shapes");
        
        
        sw.Start();
        rd.GenerateInfos(key);
        sw.Stop();
        GD.Print("generating river infos " + sw.Elapsed.TotalMilliseconds);
        sw.Reset();
        
        key.Data.Notices.SetPolyShapes?.Invoke();
        
        GD.Print("finished w riverpolytrigen");
        return rd;
    }
    private void PreprocessRiversForLandmass(TempRiverData rd, HashSet<MapPolygon> lm, GenWriteKey key)
    {
        var riverNexi = key.Data.Planet.PolyNexi.Entities
            .Where(n => n.IncidentPolys.Any(p => lm.Contains(p)))
            .Where(n => n.IncidentEdges.Any(e => e.IsRiver()));
        MakeInners(rd, riverNexi, key);
        MakePivots(rd, riverNexi, key);
    }

    private void MakeInners(TempRiverData rd, IEnumerable<MapPolyNexus> rNexi, GenWriteKey key)
    {
 
        foreach (var nexus in rNexi)
        {
            if(nexus.IsRiverNexus() == false && nexus.IncidentPolys.Any(p => p.IsWater()) == false) continue;
            var rEdges = nexus.IncidentEdges.Where(e => e.IsRiver());

            foreach (var poly in nexus.IncidentPolys)
            {
                var nexusRelative = poly.GetOffsetTo(nexus.Point, key.Data);
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

                    var intersect = Geometry.LineIntersectsLine2d(nexusSeg1.From - shift1, axis1,
                        nexusSeg2.From - shift2, axis2);
                    
                    if (intersect is Vector2 inner)
                    {
                        if (poly.PointInPolyRel(inner, key.Data) == false
                             || innerOnEdge())
                        {
                            var w = (width1 + width2) / 2f;
                            inner = nexusRelative.Normalized() * (nexusRelative.Length() - w);
                            if (poly.PointInPolyRel(inner, key.Data) == false) 
                                throw new Exception();
                            // if (innerOnEdge()) throw new Exception();
                        }

                        bool innerOnEdge()
                        {
                            var segs = poly.GetOrderedBoundarySegs(key.Data);
                            for (var i = 0; i < segs.Count; i++)
                            {
                                var seg = segs[i];
                                
                                var close = Geometry.GetClosestPointToSegment2d(inner, seg.From, seg.To);
                                if (inner.DistanceTo(close) < .01f)
                                {
                                    GD.Print("FIXING INNER ON EDGE");
                                    return true;
                                }
                            }
                            return false;
                        }
                        
                        rd.Inners.TryAdd(new PolyCornerKey(nexus, poly), inner);
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

                    var intersect = Geometry.LineIntersectsLine2d(nexusRelative + axisPerp * width / 2f, nexusEdgeAxis,
                        Vector2.Zero, nexusRelative);

                    if (intersect is Vector2 inner)
                    {
                        if (poly.PointInPolyRel(inner, key.Data) == false)
                        {
                            inner = nexusRelative.Normalized() * (nexusRelative.Length() - width);
                            if (poly.PointInPolyRel(inner, key.Data) == false)
                            {
                                var e = new GeometryException("Fixing inner failed");
                                // e.AddSegLayer();
                                throw e;
                            }
                        }

                        rd.Inners.TryAdd(new PolyCornerKey(nexus, poly), inner);
                    }
                    else throw new Exception();
                }
                else throw new Exception();
            }
        }
    }

    private void MakePivots(TempRiverData rd, IEnumerable<MapPolyNexus> rNexi, GenWriteKey key)
    {
        var rIncidentEdges = rNexi.SelectMany(n => n.IncidentEdges).Distinct();
        foreach (var edge in rIncidentEdges)
        {
            var hiSegments = edge.HighSegsRel().Segments;
            var hiPoly = edge.HighPoly.Entity();
            var hiNexRel = hiPoly.GetOffsetTo(edge.HiNexus.Entity().Point, key.Data);
            var loNexRel = hiPoly.GetOffsetTo(edge.LoNexus.Entity().Point, key.Data);

            MapPolyNexus fromNexus;
            MapPolyNexus toNexus;
            
            if (hiNexRel == hiSegments[0].From && loNexRel == hiSegments[hiSegments.Count - 1].To)
            {
                fromNexus = edge.HiNexus.Entity();
                toNexus = edge.LoNexus.Entity();
            }
            else if (loNexRel == hiSegments[0].From && hiNexRel == hiSegments[hiSegments.Count - 1].To)
            {
                fromNexus = edge.LoNexus.Entity();
                toNexus = edge.HiNexus.Entity();
            }
            else throw new Exception();
            
            if (hiSegments.Count == 1)
            {
                var seg = hiSegments[0];
                var segLength = seg.Length();
                var axis = seg.GetNormalizedAxis();
                var fromPivot = seg.From + axis * segLength * 1f / 3f;
                var toPivot = seg.From + axis * segLength * 2f / 3f;
                
                rd.HiPivots.TryAdd(new EdgeEndKey(fromNexus, edge), fromPivot);
                rd.HiPivots.TryAdd(new EdgeEndKey(toNexus, edge), toPivot);
                var offset = edge.HighPoly.Entity().Center;
                
                var split = new List<LineSegment>
                {
                    new LineSegment(seg.From, fromPivot).Translate(offset),
                    new LineSegment(fromPivot, toPivot).Translate(offset),
                    new LineSegment(toPivot, seg.To).Translate(offset)
                };
                edge.ReplacePoints(split, key);
                continue;
            }
            
            var margin = 10f;
            
            //todo make this based on rotationally neighboring r edges instead
            var newHiSegs = new List<LineSegment>();
            
            var fromSeg = hiSegments[0];
            var fromPivotWidth = fromNexus.IncidentEdges.Average(e => River.GetWidthFromFlow(e.MoistureFlow)) / 2f;
            var fromSegWidth = fromSeg.Length();
            if (fromPivotWidth + 10f >= fromSegWidth)
            {
                rd.HiPivots.TryAdd(new EdgeEndKey(fromNexus, edge), fromSeg.To);
                newHiSegs.Add(fromSeg.Copy());
            }
            else
            {
                var pivot = fromSeg.From + fromSeg.GetNormalizedAxis() * fromPivotWidth;
                rd.HiPivots.TryAdd(new EdgeEndKey(fromNexus, edge), pivot);
                var s1 = new LineSegment(fromSeg.From, pivot);
                if (s1.Length() != 0f) newHiSegs.Add(s1);
                var s2 = new LineSegment(pivot, fromSeg.To);
                if (s2.Length() != 0f) newHiSegs.Add(s2);
            }
            for (var i = 1; i < hiSegments.Count - 1; i++)
            {
                newHiSegs.Add(hiSegments[i]);
            }
            
            var toSeg = hiSegments[hiSegments.Count - 1];
            var toPivotWidth = toNexus.IncidentEdges.Average(e => River.GetWidthFromFlow(e.MoistureFlow)) / 2f;
            var toSegWidth = toSeg.Length();
            if (toPivotWidth + 10f >= toSegWidth)
            {
                rd.HiPivots.TryAdd(new EdgeEndKey(toNexus, edge), toSeg.From);
                newHiSegs.Add(toSeg.Copy());
            }
            else
            {
                var pivot = toSeg.To - toSeg.GetNormalizedAxis() * toPivotWidth;
                rd.HiPivots.TryAdd(new EdgeEndKey(toNexus, edge), pivot);
                var s1 = new LineSegment(toSeg.From, pivot);
                if (s1.Length() != 0f) newHiSegs.Add(s1);
                var s2 = new LineSegment(pivot, toSeg.To);
                if (s2.Length() != 0f) newHiSegs.Add(s2);
            }
            
            // var newAbsSegs = newHiSegs.Select(s => s.Translate(hiPoly.Center)).ToList();
            
            for (var i = 0; i < newHiSegs.Count; i++)
            {
                var seg = newHiSegs[i];
                var newFrom = seg.From + hiPoly.Center;
                var newTo = seg.To + hiPoly.Center;
                seg.From = newFrom;
                seg.To = newTo;
            }
            
            edge.ReplacePoints(newHiSegs, key);
        }
    }
}