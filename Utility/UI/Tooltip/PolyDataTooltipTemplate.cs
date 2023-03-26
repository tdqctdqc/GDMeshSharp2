using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyDataTooltipTemplate : DataTooltipTemplate<PolyTriPosition>
{
    public PolyDataTooltipTemplate() : base()
    {
    }

    protected override List<Func<PolyTriPosition, Data, Control>> _fastGetters { get; }
        = new List<Func<PolyTriPosition, Data, Control>>
        {
            GetId,
            GetRegime,
            GetLandform,
            GetVeg
        };
    protected override List<Func<PolyTriPosition, Data, Control>> _slowGetters { get; }
        = new List<Func<PolyTriPosition, Data, Control>>
        {
            GetSettlementName,
            GetSettlementSize, 
            GetPeeps,
            GetBuildings,
            GetResourceDeposits,
            GetFertility,
            GetAltitude,
        };

    private static Control GetBuildings(PolyTriPosition t, Data d)
    {
            var bs = t.Poly.GetBuildings(d);
            if (bs != null)
            {
                var label = new Label();
                var counts = bs.Select(b => b.Model.Model()).GetCounts();
                int iter = 0;
                foreach (var kvp in counts)
                {
                    if (iter != 0) label.Text += "\n";
                    iter++;
                    label.Text += $"{kvp.Key.Name} x {kvp.Value}";
                }
                return label;
            }
            return null;
    }

    private static Control GetAltitude(PolyTriPosition t, Data d)
    {
        return NodeExt.CreateLabel("Altitude: " + t.Poly.Altitude);
    }

    private static Control GetFertility(PolyTriPosition t, Data d)
    {
        var tri = t.Tri;
        if (tri == null) return null; //todo this should be fixed when the tri holes are fixed
        return NodeExt.CreateLabel("Fertility: " + t.Poly.GetFertility());
    }

    private static Control GetVeg(PolyTriPosition t, Data d)
    {
        var tri = t.Tri;
        if (tri == null) return null; //todo this should be fixed when the tri holes are fixed
        return NodeExt.CreateLabel("Landform: " + t.Tri.Vegetation.Name);
    }

    private static Control GetLandform(PolyTriPosition t, Data d)
    {
        var tri = t.Tri;
        if (tri == null) return null; //todo this should be fixed when the tri holes are fixed
        return NodeExt.CreateLabel("Landform: " + t.Tri.Landform.Name);
    }

    private static Control GetRegime(PolyTriPosition t, Data d)
    {
        var txt = "Regime: " + (t.Poly.Regime.Empty()
            ? "None"
            : t.Poly.Regime.Entity().Name);
        return NodeExt.CreateLabel(txt);
    }

    private static Control GetPeeps(PolyTriPosition t, Data d)
    {
        var peeps = t.Poly.GetPeeps(d);
        var txt = "Num Peeps: " + (peeps != null ? 
            t.Poly.GetPeeps(d).Count().ToString() 
            : "0");
        return NodeExt.CreateLabel(txt);
    }

    private static Control GetId(PolyTriPosition t, Data d)
    {
        return NodeExt.CreateLabel("Poly Id: " + t.Poly.Id.ToString());
    }

    

    private static Control GetResourceDeposits(PolyTriPosition t, Data d)
    {
    
        var rs = t.Poly.GetResourceDeposits(d);
        if (rs != null)
        {
            var label = new Label();
            int iter = 0;
            foreach (var r in rs)
            {
                if (iter != 0) label.Text += "\n";
                label.Text += $"{r.Item.Model().Name}: {Mathf.FloorToInt(r.Size)}";
            }

            return label;
        }
        return null;
    }

    private static Control GetSettlementSize(PolyTriPosition t, Data d)
    {
        return  d.Society.SettlementAux.ByPoly[t.Poly] is Settlement s
            ? NodeExt.CreateLabel("Settlement Size: " + s.Size)
            : null;
    }

    private static Control GetSettlementName(PolyTriPosition t, Data d)
    {
        return  d.Society.SettlementAux.ByPoly[t.Poly] is Settlement s
            ? NodeExt.CreateLabel("Settlement Name: " + s.Name)
            : null;
    }
}
