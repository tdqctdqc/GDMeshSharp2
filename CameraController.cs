using Godot;
using System;

public class CameraController : Camera2D
{
    private float _udScrollSpeed = 1000f;
    private Vector2 _bounds;
    private float _lrScrollSpeed = .02f;
    public float XYRatio { get; private set; }
    public override void _Ready()
    {
        Current = true;
    }

    public void SetBounds(Vector2 bounds)
    {
        _bounds = bounds;
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
            XYRatio -= delta * Zoom.Length() * _lrScrollSpeed * mult;
            if (XYRatio > 1f) XYRatio -= 1f;
            if (XYRatio < 0f) XYRatio += 1f;
        }
        if(Input.IsKeyPressed((int)KeyList.D))
        {
            XYRatio += delta * Zoom.Length() * _lrScrollSpeed * mult;
            if (XYRatio > 1f) XYRatio -= 1f;
            if (XYRatio < 0f) XYRatio += 1f;
        }


        if(Input.IsKeyPressed((int)KeyList.Z))
        {
            Zoom *= .9f * mult;
        }
        if(Input.IsKeyPressed((int)KeyList.X))
        {
            Zoom *= 1.1f * mult;
        }

        // if (Position.x < 0f)
        // {
        //     Position += new Vector2(_bounds.x, 0f);
        // }
        // if (Position.x > _bounds.x)
        // {
        //     Position -= new Vector2(_bounds.x, 0f);
        // }
    }
}