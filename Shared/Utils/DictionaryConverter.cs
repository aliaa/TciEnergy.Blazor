using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TciEnergy.Blazor.Shared.Utils
{
    public abstract class DictionaryConverter<K, V> : JsonConverter<Dictionary<K, V>>
    {
        public abstract bool TryParseKey(string str, out K value);
        public abstract bool TryParseValue(string str, out V value);

        public override Dictionary<K, V> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            var dic = new Dictionary<K, V>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return dic;

                string keyString = reader.GetString();
                if (!TryParseKey(keyString, out K key))
                    throw new JsonException($"Unable to convert key \"{keyString}\" to {typeof(K).Name}");
                
                reader.Read();
                string valueStr = reader.GetString();
                if(!TryParseValue(valueStr, out V val))
                    throw new JsonException($"Unable to convert value \"{valueStr}\" to {typeof(V).Name}");

                dic.Add(key, val);
            }

            throw new JsonException("Error Occured");
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<K, V> dic, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var item in dic)
                writer.WriteString(item.Key.ToString(), item.Value.ToString());
            writer.WriteEndObject();
        }
    }

    public class DictionaryIntConverter : DictionaryConverter<int, string>
    {
        public override bool TryParseKey(string str, out int value)
        {
            return int.TryParse(str, out value);
        }

        public override bool TryParseValue(string str, out string value)
        {
            value = str;
            return true;
        }
    }
}
