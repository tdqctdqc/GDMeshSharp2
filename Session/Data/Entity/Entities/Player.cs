using Godot;
using System;

public sealed class Player : Entity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Player(int id, string name, CreateWriteKey key) : base(id, key)
    {
        Name = name;
    }
    private static Player DeserializeConstructor(object[] args)
    {
        return new Player(args);
    }
    private Player(object[] args) : base(args) { }
}
