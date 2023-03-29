
using MessagePack;

public class DeclareWarPRocedure : Procedure
{
    public EntityRef<Regime> Declarer { get; private set; }
    public EntityRef<Regime> Declaree { get; private set; }
    public static DeclareWarPRocedure Create(Regime declarer, Regime declaree)
    {
        return new DeclareWarPRocedure(declarer.MakeRef(), declaree.MakeRef());
    }
    [SerializationConstructor] private DeclareWarPRocedure(EntityRef<Regime> declarer, EntityRef<Regime> declaree)
    {
        Declarer = declarer;
        Declaree = declaree;
    }
    public override bool Valid(Data data)
    {
        return Declarer.CheckExists(data) && Declaree.CheckExists(data);
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var relation = Declarer.Entity().RelationWith(Declaree.Entity(), key.Data);
        relation.Set<bool>(nameof(RegimeRelation.AtWar), true, key);
    }
}