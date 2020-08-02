using Newtonsoft.Json;
using System;
using Core.Models;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Core.Services
{
    public class JsonHSVConverter : JsonConverter<ColorHSV>
    {
        public override ColorHSV ReadJson(JsonReader reader, Type objectType, [AllowNull] ColorHSV existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var hsvFields = new string[3] { "H", "S", "V" };
            var jObject = JObject.Load(reader);
            var props = jObject.Properties().ToList();

            if (hsvFields.All(x => props.Any(p => p.Name.ToUpper() == x.ToUpper() && p.Value.Type == JTokenType.Integer)))
            {
                var hProp = props.First(x => x.Name.ToUpper() == "H");
                var H = (short)int.Parse(hProp.Value.ToString());

                var sProp = props.First(x => x.Name.ToUpper() == "S");
                var S = (short)int.Parse(sProp.Value.ToString());

                var vProp = props.First(x => x.Name.ToUpper() == "V");
                var V = (short)int.Parse(vProp.Value.ToString());

                return new ColorHSV(H, S, V);
            }
            else
                return new ColorHSV();
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] ColorHSV value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
