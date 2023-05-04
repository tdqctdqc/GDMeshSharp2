using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeOverviewWindow : WindowDialog
{
    private TabContainer _container;
    private RegimeConstructionOverview _construction;
    private RegimePeepsOverview _peeps;
    private RegimeProductionOverview _prod;
    private RegimeWalletOverview _wallet;
    public RegimeOverviewWindow()
    {
        RectSize = new Vector2(500f, 500f);
        _container = new TabContainer();
        _container.RectMinSize = RectSize;
        
        _construction = new RegimeConstructionOverview();
        _container.AddChild(_construction);

        _peeps = new RegimePeepsOverview();
        _container.AddChild(_peeps);

        _prod = new RegimeProductionOverview();
        _container.AddChild(_prod);

        _wallet = new RegimeWalletOverview();
        _container.AddChild(_wallet);
        AddChild(_container);
    }
    public void Setup(Regime regime, Data data)
    {
        _construction.Setup(regime, data);
        _peeps.Setup(regime, data);
        _prod.Setup(regime, data);
        _wallet.Setup(regime, data);
    }
}