using Godot;
using System;
using System.Text.Json;

[EntityProcedure]
public abstract class Procedure
{
    protected static ProcedureWriteKey Key = new ProcedureWriteKey();
}

public class ExampleProcedure : Procedure
{
    public static void EnactAndPushToServer(HostServer server, HostWriteKey key, int intField, string stringField)
    {
        var args = new ProcArgs(intField, stringField);
        Enact(Key, args);
        var update = new ProcedureUpdate(nameof(ExampleProcedure), JsonSerializer.Serialize(args));
        server.QueueUpdate(update);
    }

    public static void ReceiveFromServer(ServerWriteKey key, string json)
    {
        var args = JsonSerializer.Deserialize<ProcArgs>(json);
        Enact(Key, args);
    }
    private static void Enact(ProcedureWriteKey key, ProcArgs args)
    {
        GD.Print(args.StringField);
    }
    private class ProcArgs
    {
        public int IntField { get; set; }
        public string StringField { get; set; }
        public ProcArgs(int intField, string stringField)
        {
            IntField = intField;
            StringField = stringField;
        }
    }
}
