using System;
using System.Collections.Generic;
using System.Linq;

public class DecisionPrompt : IPrompt
{
    public string Descr { get; set; }
    public List<Action> Actions { get; set; }
    public List<string> ActionDescrs { get; set; }

    public DecisionPrompt(Decision d, ClientWriteKey key)
    {
        Descr = d.GetDescription();
        var options = d.GetOptions();
        Actions = options
            .Select(o =>
            {
                Action a = () =>
                {
                    var com = new ChooseDecisionCommand(key, d, o.Name);
                    key.Session.Server.QueueCommand(com, key);
                };
                return a;
            }).ToList();
        ActionDescrs = options.Select(o => o.Description).ToList();
    }
}
