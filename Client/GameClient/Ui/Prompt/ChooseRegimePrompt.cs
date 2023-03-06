
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChooseRegimePrompt : IPrompt
{
    public string Descr { get; private set; }
    public List<Action> Actions { get; private set; }
    public List<string> ActionDescrs { get; private set; }

    public ChooseRegimePrompt(Data data, ClientWriteKey key)
    {
        Descr = "Choose a regime";
        var availRegimes = data.Society.Regimes.Entities
            .Where(r => r.IsPlayerRegime(data) == false);
        Actions = availRegimes.Select(r => 
        {
            var com = new ChooseRegimeCommand(r.MakeRef());
            Action a = () =>
            {
                key.Session.Server.QueueCommandLocal(com, key);
            };
            return a;
        }).ToList();
        ActionDescrs = availRegimes.Select(r => $"Choose {r.Name}").ToList();
    }
}
