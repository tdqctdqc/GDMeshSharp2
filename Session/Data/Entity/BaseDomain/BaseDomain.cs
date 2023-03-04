using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain : Domain
{
    public Repository<Player> Players { get; private set; }
    public GameClockRepo GameClock { get; private set; }
    public BaseDomain(Data data) : base(data)
    {
        Players = new Repository<Player>(this, data);
        AddRepo(Players);
        GameClock = new GameClockRepo(this, data);
        AddRepo(GameClock);
    }
}
