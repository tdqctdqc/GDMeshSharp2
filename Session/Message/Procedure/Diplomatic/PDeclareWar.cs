
public class PDeclareWar : Procedure
{
    public EntityRef<Regime> Declarer { get; private set; }
    public EntityRef<Regime> Declaree { get; private set; }
    public static PDeclareWar Create(Regime declarer, Regime declaree)
    {
        return new PDeclareWar(declarer.MakeRef(), declaree.MakeRef());
    }
    private PDeclareWar(EntityRef<Regime> declarer, EntityRef<Regime> declaree)
    {
        Declarer = declarer;
        Declaree = declaree;
    }
    public override bool Valid(Data data)
    {
        return Declarer.Check(data) && Declaree.Check(data);
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var relation = Declarer.Entity().RelationWith(Declaree.Entity(), key.Data);
        relation.Set(nameof(RegimeRelation.AtWar), true, key);
    }
}
