
using System;
using System.Collections.Generic;

public interface IPrompt
{
    string Descr { get; }
    List<Action> Actions { get; }
    List<string> ActionDescrs { get; }
}

public static class IPromptExt
{
    public static void Prompt(this IPrompt p, GameClient client)
    {
        
    }
}
