using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class DebugCameraController : Camera2D
{
    private Node2D _controlled;
    private float _zoomLevel;
    private float _maxZoom = 10f, _minZoom = .1f;
    private float _zoomIncr = .1f;
    private float _scrollSpeed = 500f;
    public DebugCameraController(Node2D controlled)
    {
        _controlled = controlled;
        _zoomLevel = Zoom.x;
    }

    public override void _Process(float delta)
    {
        var mult = 1f;
        if (Input.IsKeyPressed((int) KeyList.Shift)) mult = 3f;
        if(Input.IsKeyPressed((int)KeyList.W))
        {
            _controlled.Position -= Vector2.Up * delta * Zoom * _scrollSpeed * mult;
        }
        if(Input.IsKeyPressed((int)KeyList.S))
        {
            _controlled.Position -= Vector2.Down * delta * Zoom * _scrollSpeed * mult;
        }
        if(Input.IsKeyPressed((int)KeyList.A))
        {
            _controlled.Position -= Vector2.Left * delta * Zoom * _scrollSpeed * mult;
        }
        if(Input.IsKeyPressed((int)KeyList.D))
        {
            _controlled.Position -= Vector2.Right * delta * Zoom * _scrollSpeed * mult;
        }
        
        if(Input.IsKeyPressed((int)KeyList.Z))
        {
            _zoomLevel -= _zoomIncr;
            UpdateZoom();

        }
        if(Input.IsKeyPressed((int)KeyList.X))
        {
            _zoomLevel += _zoomIncr;
            UpdateZoom();

        }

    }
    
    private void UpdateZoom()
    {
        _controlled.Scale = Vector2.One * _zoomLevel;
    }
}
