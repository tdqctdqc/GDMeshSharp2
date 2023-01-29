using Godot;
using System;
using System.Text.Json.Serialization;

public sealed class Player : Entity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Player(int id, string name, CreateWriteKey key) : base(id, key)
    {
        Name = name;
    }

    [JsonConstructor] public Player(int id, Guid userId, string name) : base(id)
    {
        UserId = userId;
        Name = name;
    }
}
