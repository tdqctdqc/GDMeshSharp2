using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapInputCatcher : Node
{
    private MapGraphics _graphics;
    private Data _data;
    private MouseOverPolyHandler _mouseOverHandler;
    public MapInputCatcher(Data data, MapGraphics graphics)
    {
        _data = data;
        _graphics = graphics;
        _mouseOverHandler = new MouseOverPolyHandler();
    }

    private MapInputCatcher()
    {
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventMouseMotion mm)
        {
            var mapPos = Game.I.Client.Cam.GetMousePosInMapSpace();
            var d = GetProcessDeltaTime();
            _mouseOverHandler.Process(d, _data, mapPos);
            // GetViewport().SetInputAsHandled();
        }

        if (e is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == (int) ButtonList.Right)
            {
                TryOpenRegimeOverview();
            }
        }

        Game.I.Client.Cam.Process(e);
    }


    private void TryOpenRegimeOverview()
    {
        var poly = _mouseOverHandler.MouseOverPoly;
        if (poly.Regime.Fulfilled())
        {
            var r = poly.Regime.Entity();
            var w = Game.I.Client.Requests.OpenWindow<RegimeOverviewWindow>();
            w.Setup(r, _data);
        }
    }
}
