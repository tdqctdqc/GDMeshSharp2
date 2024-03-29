using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain : Domain
{
    public EntityRegister<Player> Players => GetRegister<Player>();
    public PlayerAux PlayerAux { get; private set; }
    public GameClock GameClock => _gameClockAux.Value;
    private GameClockAux _gameClockAux;
    public RuleVars Rules => _ruleVarsAux.Value;
    private SingletonAux<RuleVars> _ruleVarsAux;
    public BaseDomain(Data data) : base(typeof(BaseDomain), data)
    {
        
    }
    public override void Setup()
    {
        PlayerAux = new PlayerAux(this, Data);
        _gameClockAux = new GameClockAux(this, Data);
        _ruleVarsAux = new SingletonAux<RuleVars>(this, Data);
    }
}
