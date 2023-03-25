
public class EntityDestroyedHandler<TEntity> : EntityNoticeHandler<TEntity, EntityDestroyedNotice<TEntity>>
    where TEntity : Entity
{
    public static void Raise(TEntity t)
    {
        //todo raise for parent types up to repo type as well
        //todo check if anyone registered before creating notice 
        Raise(new EntityDestroyedNotice<TEntity>(t));
    }
}
