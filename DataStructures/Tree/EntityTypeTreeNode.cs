using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class EntityTypeTreeNode 
{
    public Type Value { get; private set; }
    public EntityTypeTreeNode Parent { get; private set; }
    public Dictionary<string, entva> I { get; private set; }
    public List<EntityTypeTreeNode> Children { get; private set; }
    private Dictionary<Type, RefAction<IEntityNotice>> _publishersByNoticeType;
    

    public EntityTypeTreeNode(Type value)
    {
        Value = value;
        _publishersByNoticeType = new Dictionary<Type, RefAction<IEntityNotice>>();
        Children = new List<EntityTypeTreeNode>();
    }
    
    public void Propagate(IEntityNotice notice)
    {
        if (notice.EntityType != Value) throw new Exception();
        BubbleUp(notice);
        Publish(notice);
        BubbleDown(notice);
    }

    private void Publish(IEntityNotice notice)
    {
        _publishersByNoticeType[notice.GetType()].Invoke(notice);
    }
    public void Register<TNotice>(Action<TNotice> callback) where TNotice : IEntityNotice
    {
        var noticeType = typeof(TNotice);
        if (_publishersByNoticeType.ContainsKey(noticeType) == false)
        {
            _publishersByNoticeType.Add(noticeType, new RefAction<IEntityNotice>());
        }
        _publishersByNoticeType[noticeType].Subscribe(a => callback((TNotice)a));
    }

    public void RegisterForValueChange<TProperty>(string fieldName, Action<ValChangeNotice<TProperty>> callback)
    {
        EntityValChangedHandler<asdfwaef>
    }
    private void BubbleUp(IEntityNotice notice)
    {
        if (Value.IsAssignableFrom(notice.EntityType)) return;
        if (Parent != null)
        {
            ((EntityTypeTreeNode)Parent).BubbleUp(notice);
        }
        Publish(notice);
    }

    private void BubbleDown(IEntityNotice notice)
    {
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
    public void SetParent(EntityTypeTreeNode parent)
    {
        if (Parent != null) Parent.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
    }
}
