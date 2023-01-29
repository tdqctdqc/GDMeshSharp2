using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

public class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Read();
        reader.Read();
        float r = float.Parse(reader.GetString());

        reader.Read();
        reader.Read();
        float g = float.Parse(reader.GetString());
        
        reader.Read();
        reader.Read();
        float b = float.Parse(reader.GetString());
        
        reader.Read();
        reader.Read();
        float a = float.Parse(reader.GetString());
        
        reader.Read();
        
        return new Color(r, g, b, a);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("r", value.r.ToString());
        writer.WriteString("g", value.g.ToString());
        writer.WriteString("b", value.b.ToString());
        writer.WriteString("a", value.a.ToString());
        writer.WriteEndObject();
    }
}