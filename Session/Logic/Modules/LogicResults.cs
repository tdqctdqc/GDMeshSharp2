
using System.Collections.Generic;
using System.Linq;

public class LogicResults
{
    public List<Procedure> Procedures { get; private set; }
    public List<Decision> Decisions { get; private set; }
    public List<Update> Updates { get; private set; }

    public LogicResults(IEnumerable<Message> messages)
    {
        Updates = messages.SelectWhereOfType<Message, Update>().ToList();
        Procedures = messages.SelectWhereOfType<Message, Procedure>().ToList();
        Decisions = messages.SelectWhereOfType<Message, Decision>().ToList();
    }
}
