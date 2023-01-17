using Godot;
using System;
[EntityVariable]
public class EntityId 
{
    public string Name { get; private set; }
    public int Value { get; private set; }

    private EntityId(int value)
    {
        Name = "Id";
        Value = value;
    }

    public void Update(HostWriteKey key, int newValue, IServer server)
    {
        throw new Exception();
    }
    public static void ReceiveUpdate(EntityId str, ServerWriteKey key, string newValueJson)
    {
        throw new Exception();
    }

    public void SetByProcedure(ProcedureWriteKey key, int newValue)
    {
        throw new Exception();
    }
    
    public static EntityId Construct(int value)
    {
        return new EntityId(value);
    }
    public static string Serialize(EntityId es)
    {
        return es.Value.ToString();
    }

    public static EntityId Deserialize(string json, string name, Entity entity)
    {
        var value = System.Text.Json.JsonSerializer.Deserialize<int>(json);
        return Construct(value);
    }
}
