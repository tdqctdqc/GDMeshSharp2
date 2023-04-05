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

        if (e is InputEventKey k && k.Pressed == false)
        {
            if (k.Scancode == (int) KeyList.B)
            {
                TryConstruct();
            }
        }
        Game.I.Client.Cam.Process(e);
    }

    private void TryConstruct()
    {
        var tri = _mouseOverHandler.MouseOverTri;
        if (tri == null) return;
        if (tri.Landform.IsLand)
        {
            var farm = BuildingModelManager.Farm;
            var regime = _data.BaseDomain.PlayerAux.LocalPlayer.Regime;
            if (regime.Empty()) return;
            var proc = StartConstructionProcedure.Construct
            (
                ((BuildingModel) farm).MakeRef(),
                new PolyTriPosition(_mouseOverHandler.MouseOverPoly.Id, _mouseOverHandler.MouseOverTri.Index),
                regime
            );
            var com = new ProcedureCommand(proc);
            Game.I.Client.Requests.QueueCommand.Invoke(com);
        }
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
