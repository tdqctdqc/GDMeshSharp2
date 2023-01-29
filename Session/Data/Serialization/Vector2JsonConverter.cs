using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

public class Vector2JsonConverter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, 
        JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();
        float x = reader.GetSingle();

        reader.Read();
        reader.Read();
        float y = reader.GetSingle();
        reader.Read();
        
        return new Vector2(x, y);
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("x", value.x);
        writer.WriteNumber("y", value.y);
        writer.WriteEndObject();
    }
}