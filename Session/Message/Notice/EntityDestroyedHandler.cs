
public class EntityDestroyedHandler<TEntity> : EntityNoticeHandler<TEntity, EntityDestroyedNotice<TEntity>>
    where TEntity : Entity
{
    public static void Raise(TEntity t)
    {
        Raise(new EntityDestroyedNotice<TEntity>(t));
    }
}
