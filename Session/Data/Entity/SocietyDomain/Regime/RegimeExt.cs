
public static class RegimeExt
{
    public static RegimeRelation RelationWith(this Regime r1, Regime r2, Data data)
    {
        return data.Society.Relations.ByRegime[r1, r2];
    }

    public static bool IsPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.Players.ByRegime.ContainsKey(r);
    }
    public static bool IsLocalPlayerRegime(this Regime r, Data data)
    {
        return data.BaseDomain.Players.LocalPlayer.Regime.Entity() == r;
    }
    public static Player GetPlayer(this Regime r, Data data)
    {
        return data.BaseDomain.Players.ByRegime[r];
    }
}
