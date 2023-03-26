using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain : Domain
{
    public EntityRegister<Player> PlayersR => GetRegister<Player>();
    public EntityRegister<GameClock> GameClockR => GetRegister<GameClock>();
    public EntityRegister<RuleVars> RuleVarsR => GetRegister<RuleVars>();
    public PlayerRepo Players { get; private set; }
    public GameClockRepo GameClock { get; private set; }
    public SingletonRepo<RuleVars> RuleVars { get; private set; }
    public BaseDomain(Data data) : base(data, typeof(BaseDomain))
    {
        Players = new PlayerRepo(this, data);
        AddRepo(Players);
        GameClock = new GameClockRepo(this, data);
        AddRepo(GameClock);
        RuleVars = new SingletonRepo<RuleVars>(this, data);
        AddRepo(RuleVars);
    }
}
