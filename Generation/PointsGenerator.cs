using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class PointsGenerator 
{
    public static List<Vector2> GetSquareMarkerMesh(List<Vector2> points, float markerSize)
    {
        var list = new List<Vector2>();
        foreach (var p in points)
        {
            var topLeft = p + Vector2.Up * markerSize / 2f
                            + Vector2.Left * markerSize / 2f;
            var topRight = p + Vector2.Up * markerSize / 2f
                            + Vector2.Right * markerSize / 2f;
            var bottomLeft = p + Vector2.Down * markerSize / 2f
                            + Vector2.Left * markerSize / 2f;
            var bottomRight = p + Vector2.Down * markerSize / 2f
                            + Vector2.Right * markerSize / 2f;
            list.Add(topLeft);
            list.Add(topRight);
            list.Add(bottomLeft);
            list.Add(topRight);
            list.Add(bottomRight);
            list.Add(bottomLeft);
        }

        return list;
    }
    public static List<Vector2> GenerateSemiRegularPoints(Vector2 dim, 
                                                    float cellSize, 
                                                    int triesBeforeExit,
                                                    bool square,
                                                    int maxPoints = int.MaxValue)
    {
        var points = new List<Vector2>();
        var grid = new RegularGrid<Vector2>(v => v, cellSize);
        var rand = new RandomNumberGenerator();
        int badTries = 0;

        while(badTries < triesBeforeExit && points.Count < maxPoints)
        {
            float x = rand.RandfRange(0f, dim.x);
            float y = rand.RandfRange(0f, dim.y);
            var vec = new Vector2(x,y);
            if(grid.GetElementsAtPoint(vec).Count == 0)
            {
                badTries = 0;
                grid.AddElement(vec);
                points.Add(vec);
            }
            else
            {
                badTries++;
            }
        }
        if(square)
        {
            AddMeshBorder(points, dim, cellSize, cellSize / 2f);
        }
        return points;
    }

    public static List<Vector2> GenerateConstrainedSemiRegularPoints(Vector2 dim,
                                                                float cellSize,
                                                                float constraintSize,
                                                                bool square,
                                                                bool roundToInt)
    {
        var rand = new RandomNumberGenerator();
        var points = new List<Vector2>();
        int xCells = Mathf.CeilToInt(dim.x / cellSize);
        int yCells = Mathf.CeilToInt(dim.y / cellSize);
        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < yCells; j++)
            {
                var cellCenter = Vector2.One * cellSize / 2f + new Vector2(i,j) * cellSize;
                float x = rand.RandfRange(-constraintSize / 2f, constraintSize / 2f);
                float y = rand.RandfRange(-constraintSize / 2f, constraintSize / 2f);
                var point = cellCenter + new Vector2(x,y);
                if (roundToInt) point = new Vector2((int) point.x, (int) point.y);
                points.Add(point);
            }
        }
        if(square) AddMeshBorder(points, dim, cellSize, cellSize / 2f);
        return points;
    }

    private static void SquareMesh(List<Vector2> points, Vector2 dim)
    {
        var tl = Vector2.Zero;
        if(points.Contains(tl) == false) points.Add(tl);

        var tr = Vector2.Right * dim.x;
        if(points.Contains(tr) == false) points.Add(tr);

        var bl = Vector2.Down * dim.y;
        if(points.Contains(bl) == false) points.Add(bl);

        var br = dim;
        if(points.Contains(br) == false) points.Add(br);
    }
    private static void AddMeshBorder(List<Vector2> points, 
                                        Vector2 dim, 
                                        float cellSize,
                                        float margin)
    {
        int xPoints = Mathf.CeilToInt((dim.x + margin * 2f) / cellSize);
        int yPoints = Mathf.CeilToInt((dim.y + margin * 2f) / cellSize);

        for (int i = 0; i < xPoints; i++)
        {
            var top = new Vector2(i * cellSize, 0f) - Vector2.One * margin;
            var bottom = new Vector2(i * cellSize, dim.y) + Vector2.One * margin;
            if(points.Contains(top) == false) points.Add(top);
            if(points.Contains(bottom) == false) points.Add(bottom);
        }
        for (int i = 0; i < yPoints; i++)
        {
            var left = new Vector2(0f, i * cellSize) - Vector2.One * margin;
            var right = new Vector2(dim.x, i * cellSize) + Vector2.One * margin;
            if(points.Contains(left) == false) points.Add(left);
            if(points.Contains(right) == false) points.Add(right);
        }
        var bl = new Vector2(-margin, cellSize * (yPoints - .5f));
        var tr = new Vector2(cellSize * (xPoints - .5f), -margin);
        if(points.Contains(bl) == false) points.Add(bl);
        if(points.Contains(tr) == false) points.Add(tr);
    }

    public static List<Vector2> GetCircleOutlinePoints(Vector2 center, float radius, int resolution)
    {
        var points = new List<Vector2>();
        for (int i = 0; i < 10; i++)
        {
            points.Add(radius * Vector2.Up.Rotated(-i * (Mathf.Pi * 2f / 20f)));
        }
        return points;
    }

    public static List<Vector2> GenerateInteriorPoints(this IReadOnlyList<LineSegment> border, float cellSize, float margin)
    {
        var res = new List<Vector2>();

        var minX = border.Min(b => Mathf.Min(b.From.x, b.To.x));
        var maxX = border.Max(b => Mathf.Max(b.From.x, b.To.x));
        
        var minY = border.Min(b => Mathf.Min(b.From.y, b.To.y));
        var maxY = border.Max(b => Mathf.Max(b.From.y, b.To.y));

        var xCells = Mathf.Abs(maxX - minX) / cellSize;
        var yCells = Mathf.Abs(maxY - minY) / cellSize;
        var mod = Vector2.Right * cellSize * .1f;
        var shift = Vector2.One * cellSize * .5f;

        for (int i = 0; i < xCells; i++)
        {
            for (int j = 0; j < yCells; j++)
            {
                mod = mod.Rotated(Game.I.Random.RandfRange(0f, Mathf.Pi * 2f));
                var p = new Vector2(minX + cellSize * i, minY + cellSize * j) + mod + shift;
                
                foreach (var b in border)
                {
                    // if(b.DistanceTo(p) > 500f) GD.Print("dist is " + b.DistanceTo(p));
                }
                if(border.All(b => 
                        b.LeftOf(p)
                        && b.DistanceTo(p) > margin
                        )
                   )
                {
                    res.Add(p);
                }
            }
        }
        return res;
    }
    
    public static List<Vector2> GeneratePoissonPoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection = 30) {
		float cellSize = radius/Mathf.Sqrt(2);

		int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x/cellSize), Mathf.CeilToInt(sampleRegionSize.y/cellSize)];
		List<Vector2> points = new List<Vector2>();
		List<Vector2> spawnPoints = new List<Vector2>();

		spawnPoints.Add(sampleRegionSize/2);
		while (spawnPoints.Count > 0) {
			int spawnIndex = Game.I.Random.RandiRange(0,spawnPoints.Count - 1);
			Vector2 spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < numSamplesBeforeRejection; i++)
			{
				float angle = Game.I.Random.Randf() * Mathf.Pi * 2;
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				Vector2 candidate = spawnCentre + dir * Game.I.Random.RandfRange(radius, 2*radius);
				if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid)) {
					points.Add(candidate);
					spawnPoints.Add(candidate);
					grid[(int)(candidate.x/cellSize),(int)(candidate.y/cellSize)] = points.Count;
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted) {
				spawnPoints.RemoveAt(spawnIndex);
			}

		}

		return points;
	}

	private static bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid) {
		if (candidate.x >=0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y) {
			int cellX = (int)(candidate.x/cellSize);
			int cellY = (int)(candidate.y/cellSize);
			int searchStartX = Mathf.Max(0,cellX -2);
			int searchEndX = Mathf.Min(cellX+2,grid.GetLength(0)-1);
			int searchStartY = Mathf.Max(0,cellY -2);
			int searchEndY = Mathf.Min(cellY+2,grid.GetLength(1)-1);

			for (int x = searchStartX; x <= searchEndX; x++) {
				for (int y = searchStartY; y <= searchEndY; y++) {
					int pointIndex = grid[x,y]-1;
					if (pointIndex != -1) {
						float sqrDst = (candidate - points[pointIndex]).LengthSquared();
						if (sqrDst < radius*radius) {
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
}