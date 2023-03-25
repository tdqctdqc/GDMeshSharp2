using System;
using System.Collections.Generic;
using System.Linq;

public class Logger
{
    public Dictionary<LogType, List<string>> Logs { get; private set; }

    public Logger()
    {
        Logs = new Dictionary<LogType, List<string>>();
    }
    
    public void Log(string msg, LogType logType)
    {
        Logs.AddOrUpdate(logType, msg);
    }
}
