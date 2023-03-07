
using System;

public class EntityCreatedHandler<TEntity> 
    : EntityNoticeHandler<TEntity, EntityCreatedNotice<TEntity>>
    where TEntity : Entity
{
}
