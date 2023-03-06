
public class EntityCreatedNotice<TEntity> 
    where TEntity : Entity
{
    public TEntity Entity { get; private set; }

    public EntityCreatedNotice(TEntity entity)
    {
        Entity = entity;
    }
}
