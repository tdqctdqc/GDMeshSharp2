
public class EntityDestroyedNotice<TEntity> 
{
    public TEntity Entity { get; private set; }

    public EntityDestroyedNotice(TEntity entity)
    {
        Entity = entity;
    }
}
