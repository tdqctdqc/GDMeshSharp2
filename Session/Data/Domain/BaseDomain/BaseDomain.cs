using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain : Domain
{
    public Repository<Player> Players { get; private set; }
    public BaseDomain() : base()
    {
        Players = new Repository<Player>(this);
        _repos.Add(typeof(Player), Players);
    }
}
