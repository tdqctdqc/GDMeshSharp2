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
        MakePivotsNew(riverNexi, key);
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

    private void MakePivotsNew(IEnumerable<MapPolyNexus> rNexi, GenWriteKey key)
    {
        var rd = key.Data.Planet.GetRegister<TempRiverData>().Entities.First();
        var rIncidentEdges = rNexi.SelectMany(n => n.IncidentEdges).Distinct().ToList();
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
                
                rd.HiPivots.Add(new EdgeEndKey(fromNexus, edge), fromPivot);
                rd.HiPivots.Add(new EdgeEndKey(toNexus, edge), toPivot);
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
                rd.HiPivots.Add(new EdgeEndKey(fromNexus, edge), fromSeg.To);
                newHiSegs.Add(fromSeg.Copy());
            }
            else
            {
                var pivot = fromSeg.From + fromSeg.GetNormalizedAxis() * fromPivotWidth;
                rd.HiPivots.Add(new EdgeEndKey(fromNexus, edge), pivot);
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
                rd.HiPivots.Add(new EdgeEndKey(toNexus, edge), toSeg.From);
                newHiSegs.Add(toSeg.Copy());
            }
            else
            {
                var pivot = toSeg.To - toSeg.GetNormalizedAxis() * toPivotWidth;
                rd.HiPivots.Add(new EdgeEndKey(toNexus, edge), pivot);
                var s1 = new LineSegment(toSeg.From, pivot);
                if (s1.Length() != 0f) newHiSegs.Add(s1);
                var s2 = new LineSegment(pivot, toSeg.To);
                if (s2.Length() != 0f) newHiSegs.Add(s2);
            }


            var otherSegs = hiPoly.GetEdges(key.Data).Where(e => e != edge)
                .SelectMany(e => e.GetSegsRel(hiPoly).Segments);
            
            foreach (var otherSeg in otherSegs)
            {
                for (var i = 0; i < newHiSegs.Count - 1; i++)
                {
                    var thisSeg = newHiSegs[i];
                    if (thisSeg.From == otherSeg.To && thisSeg.To == otherSeg.From)
                    {
                        var e = new SegmentsException("retracking boundary seg");
                        e.AddSegLayer(hiSegments, "source");
                        e.AddSegLayer(newHiSegs, "new");
                        throw e;
                    }
                }
            }
            
            
            
            var newAbsSegs = newHiSegs.Select(s => s.Translate(hiPoly.Center)).ToList();
            edge.ReplacePoints(newAbsSegs, key);
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
                var oldHiSegs = edge.HighSegsRel().Segments.Select(s => new LineSegment(s.From, s.To)).ToList();
                var oldPoints = edge.HighSegsRel().Segments.GetPoints().ToList();
                var from = oldPoints[0];
                var to = oldPoints[oldPoints.Count - 1];
                var axis = to - from;

                
                var cornerKey = new EdgeEndKey(nexus, edge);
                var k = new PolyCornerKey(nexus, edge.HighPoly.Entity());
                var hiPolyInner = rd.Inners[k];
                var loPolyInner = rd.Inners[new PolyCornerKey(nexus, edge.LowPoly.Entity())];
                var loInnerRelToHi = loPolyInner + edge.HighPoly.Entity().GetOffsetTo(edge.LowPoly.Entity(), key.Data);
                var pivot = (hiPolyInner + loInnerRelToHi) / 2f;

                var projL = (pivot - from).GetProjectionLength(axis);
                if (projL < 0f || projL > axis.Length())
                {
                    GD.Print("axis length " + axis.Length());
                    GD.Print("proj length " + projL);
                    throw new Exception();
                }
                
                rd.HiPivots.Add(new EdgeEndKey(nexus, edge), pivot);
                
                
                var edgeLength = from.DistanceTo(to);
                oldPoints.Add(pivot);
                oldPoints = oldPoints.Where(p =>
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
                if (oldPoints[0] != from) throw new Exception();   
                if (oldPoints[oldPoints.Count - 1] != to) throw new Exception();
                
                var newHiSegs = new List<LineSegment>();
                for (var i = 0; i < oldPoints.Count - 1; i++)
                {
                    if (oldPoints[i] == oldPoints[i + 1]) continue;
                    newHiSegs.Add(new LineSegment(oldPoints[i], oldPoints[i + 1]));
                }
                
                var newAbsSegs = newHiSegs.Select(s => s.Translate(edge.HighPoly.Entity().Center))
                    .Where(ls => ls.From != ls.To)
                    .ToList();
                
                edge.ReplacePoints(newAbsSegs, key);
            }
        }
    }
}