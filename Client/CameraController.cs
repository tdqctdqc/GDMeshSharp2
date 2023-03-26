using Godot;
using System;

public class CameraController : Camera2D, ICameraController
{
    private float _udScrollSpeed = 1000f;
    private float _lrScrollSpeed = .02f;
    public float XScrollRatio { get; private set; }
    private Data _data;

    public static CameraController Construct(Data data)
    {
        var c = new CameraController();
        c.Setup(data);
        return c;
    }
    private CameraController()
    {
        
    }
    public void Setup(Data data)
    {
        _data = data;
    }
    
    public Vector2 GetMousePosInMapSpace()
    {
        if(_data.Planet == null) return Vector2.Inf;
        if(_data.Planet.PlanetInfo == null) return Vector2.Inf;
        if(_data.Planet.PlanetInfo.Entities.Count == 0) return Vector2.Inf;
        
        var mapWidth = _data.Planet.Width;
        var scrollDist = mapWidth * XScrollRatio;
        
        var mousePosGlobal = GetGlobalMousePosition();
        var mapSpaceMousePos =  new Vector2(mousePosGlobal.x + scrollDist, mousePosGlobal.y);
        while (mapSpaceMousePos.x > mapWidth) mapSpaceMousePos += Vector2.Left * mapWidth;
        while (mapSpaceMousePos.x < 0f) mapSpaceMousePos += Vector2.Right * mapWidth;
        return mapSpaceMousePos;
    }

    public Vector2 GetMapPosInGlobalSpace(Vector2 mapPos)
    {
        var mapWidth = _data.Planet.Width;   
        var scrollDist = mapWidth * XScrollRatio;
        
        var globalSpace = new Vector2(mapPos.x - scrollDist, mapPos.y);
        
        while (globalSpace.x > mapWidth / 2f) globalSpace += Vector2.Left * mapWidth;
        while (globalSpace.x < -mapWidth / 2f) globalSpace += Vector2.Right * mapWidth;
        
        return globalSpace;
    }
    
    public override void _Process(float delta)
    {
        var mult = 1f;
        if (Input.IsKeyPressed((int) KeyList.Shift)) mult = 3f;
        if(Input.IsKeyPressed((int)KeyList.W))
        {
            Position += Vector2.Up * delta * Zoom * _udScrollSpeed * mult;
        }
        if(Input.IsKeyPressed((int)KeyList.S))
        {
            Position += Vector2.Down * delta * Zoom * _udScrollSpeed * mult;
        }
        
        
        if(Input.IsKeyPressed((int)KeyList.A))
        {
            XScrollRatio -= delta * Zoom.Length() * _lrScrollSpeed * mult;
            if (XScrollRatio > 1f) XScrollRatio -= 1f;
            if (XScrollRatio < 0f) XScrollRatio += 1f;
        }
        if(Input.IsKeyPressed((int)KeyList.D))
        {
            XScrollRatio += delta * Zoom.Length() * _lrScrollSpeed * mult;
            if (XScrollRatio > 1f) XScrollRatio -= 1f;
            if (XScrollRatio < 0f) XScrollRatio += 1f;
        }
        
        if(Input.IsKeyPressed((int)KeyList.Q))
        {
            Position += Vector2.Left * delta * Zoom * _udScrollSpeed * mult;
        }
        if(Input.IsKeyPressed((int)KeyList.E))
        {
            Position += Vector2.Right * delta * Zoom * _udScrollSpeed * mult;
        }

        if(Input.IsKeyPressed((int)KeyList.Z))
        {
            Zoom *= .9f;
            Zoom = new Vector2(
                Mathf.Clamp(Zoom.x, .1f, 100f),
                Mathf.Clamp(Zoom.y, .1f, 100f)
            );
        }
        if(Input.IsKeyPressed((int)KeyList.X))
        {
            Zoom *= 1.1f;
            Zoom = new Vector2(
                Mathf.Clamp(Zoom.x, .1f, 100f),
                Mathf.Clamp(Zoom.y, .1f, 100f)
            );
        }
    }
}