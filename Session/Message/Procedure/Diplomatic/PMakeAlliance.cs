public class PMakeAlliance : Procedure
{
    public EntityRef<Regime> Offerer { get; private set; }
    public EntityRef<Regime> Accepter { get; private set; }

    public static PMakeAlliance Create(Regime declarer, Regime declaree)
    {
        return new PMakeAlliance(declarer.MakeRef(), declaree.MakeRef());
    }
    private PMakeAlliance(EntityRef<Regime> offerer, EntityRef<Regime> accepter)
    {
        Offerer = offerer;
        Accepter = accepter;
    }
    public override bool Valid(Data data)
    {
        return Offerer.Check(data) && Accepter.Check(data);
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var relation = Offerer.Entity().RelationWith(Accepter.Entity(), key.Data);
        relation.Set(nameof(RegimeRelation.Alliance), true, key);
    }
}