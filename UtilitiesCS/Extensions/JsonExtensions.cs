using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilitiesCS.Extensions
{
    public static class JsonExtensions
    {
        public static T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }

        public static string ToJsonText(this JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var sb = new StringBuilder();
            var sw = new StringWriter(sb);
            var jsonWriter = new JsonTextWriter(sw);

            jsonWriter.WriteToken(reader);

            return sb.ToString();
        }
    }
}
