using Godot;
using System;
using System.Collections.Generic;

public class CameraController : Camera2D, ICameraController
{
    private float _udScrollSpeed = 1000f;
    private float _lrScrollSpeed = .02f;
    private float _zoomIncr = .01f;
    private float _zoomLevel = 1f;
    private float _maxZoom = 50f;
    private float _minZoom = .5f;
    private float _minZoomLevel = .05f;
    private float _maxZoomLevel = .9f;
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
        UpdateZoom();
    }
    public void Setup(Data data)
    {
        _data = data;
    }
    
    public Vector2 GetMousePosInMapSpace()
    {
        if(_data.Planet == null) return Vector2.Inf;
        if(_data.Planet.Info == null) return Vector2.Inf;
        
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

    public void Process(InputEvent e)
    {
        if (e is InputEventMouseButton mb)
        {
            HandleMouseButton(mb);
        }
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
    }

    
    private void HandleMouseButton(InputEventMouseButton mb)
    {
        if(mb.ButtonIndex == (int)ButtonList.WheelUp)
        {
            _zoomLevel -= _zoomIncr;
        }
        if(mb.ButtonIndex == (int)ButtonList.WheelDown)
        {
            _zoomLevel += _zoomIncr;
        }

        UpdateZoom();
    }
    
    private void UpdateZoom()
    {
        _zoomLevel = Mathf.Clamp(_zoomLevel, _minZoomLevel, _maxZoomLevel);
        var zoomFactor = ShapingFunctions.EaseInCubic(_zoomLevel, _maxZoom, _minZoom);
        zoomFactor = Mathf.Clamp(zoomFactor, _minZoom, _maxZoom);
        Zoom = Vector2.One * zoomFactor;
    }
}