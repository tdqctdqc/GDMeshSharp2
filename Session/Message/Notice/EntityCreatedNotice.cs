
using System;

public class EntityCreatedNotice : IEntityNotice
{
    public Entity Entity { get; private set; }
    Type IEntityNotice.EntityType => Entity.GetType();

    public EntityCreatedNotice(Entity entity)
    {
        Entity = entity;
    }
}
