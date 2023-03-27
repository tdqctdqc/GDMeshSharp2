
public static class RegimeExt
{
    public static RegimeRelation RelationWith(this Regime r1, Regime r2, Data data)
    {
        return data.Society.RelationAux.ByRegime[r1.Id, r2.Id];
    }

    public static bool IsPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime.ContainsKey(r);
    }
    public static bool IsLocalPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity() == r;
    }
    public static Player GetPlayer(this Regime r, Data data)
    {
        return data.BaseDomain.PlayerAux.ByRegime[r];
    }
}
