using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class Culture : IModel
{
    public string Name { get; }
    public List<string> SettlementNames { get; private set; }
    public List<RegimeTemplate> RegimeTemplates { get; private set; }

    public Culture(string json)
    {
        var d = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        Name = d[nameof(Name)];
        RegimeTemplates = JsonSerializer
            .Deserialize<string[]>(d[nameof(RegimeTemplates)])
            .Select(s => new RegimeTemplate(this, s))
            .ToList();
        SettlementNames = JsonSerializer
            .Deserialize<List<string>>(d[nameof(SettlementNames)]);

    }
}
