using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityTypeTreeNode
{
    public IEntityMeta Meta { get; private set; }
    public Type Value { get; private set; }
    public EntityTypeTreeNode Parent { get; private set; }
    public List<EntityTypeTreeNode> Children { get; private set; }
    public RefAction<EntityCreatedNotice> Created { get; private set; }    
    public RefAction<EntityDestroyedNotice> Destroyed { get; private set; }
    public EntityValChangeHandler EntityValChanged { get; private set; }
    
    public EntityTypeTreeNode(Type value)
    {
        Meta = Game.I.Serializer.GetEntityMeta(value);
        Value = value;
        Children = new List<EntityTypeTreeNode>();
        Created = new RefAction<EntityCreatedNotice>();
        Destroyed = new RefAction<EntityDestroyedNotice>();
        EntityValChanged = new EntityValChangeHandler(value);
        value
            .GetProperty(nameof(RoadSegment.EntityTypeTreeNode), 
                BindingFlags.Public | BindingFlags.Static)
            .SetMethod
            .Invoke(null, new object[]{this});
    }
    
    public void Propagate(IEntityNotice notice)
    {
        if (notice.EntityType != Value)
        {
            throw new NoticeException($"notice entity type {notice.EntityType.Name} is not {Value.Name}");
        }
        Parent?.BubbleUp(notice);
        Publish(notice);
        for (var i = 0; i < Children.Count; i++)
        {
            if (notice.EntityType.IsAssignableFrom(Children[i].Value))
            {
                Children[i].BubbleDown(notice);
                break;
            }
        }
    }
    
    private void Publish(IEntityNotice notice)
    {
        //todo maybe make dic 
        if (notice is ValChangeNotice v)
        {
            EntityValChanged.HandleChange(v);
            return;
        }
        if (notice is EntityCreatedNotice c)
        {
            Created.Invoke(c);
            return;
        }
        if (notice is EntityDestroyedNotice d)
        {
            Destroyed.Invoke(d);
            return;
        }
    }

    private bool RelevantField(IEntityNotice n)
    {
        return n is ValChangeNotice v == false || Meta.FieldNameHash.Contains(v.FieldName);
    }
    private void BubbleUp(IEntityNotice notice)
    {
        if (RelevantField(notice) == false) return;
        Parent?.BubbleUp(notice);
        Publish(notice);
    }
    private void BubbleDown(IEntityNotice notice)
    {
        Publish(notice);
        for (var i = 0; i < Children.Count; i++)
        {
            if (Children[i].Value.IsAssignableFrom(notice.EntityType))
            {
                Children[i].BubbleDown(notice);
                break;
            }
        }
    }
    public void SetParent(EntityTypeTreeNode parent)
    {
        if (Parent != null) Parent.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
    }
}
