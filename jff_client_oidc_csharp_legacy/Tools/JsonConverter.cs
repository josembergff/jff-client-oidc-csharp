using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace jff_client_oidc_csharp_legacy.Tools
{
    public static class JsonConverter
    {
        public static T JsonDeserializer<T>(string jsonString)
        {
            T deserializedUser = default(T);
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            var ser = new DataContractJsonSerializer(typeof(T));
            deserializedUser = (T)ser.ReadObject(ms);// as T;
            ms.Close();
            return deserializedUser;
        }

        public static String JsonSerializer<T>(T t)
        {
            var stream1 = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(stream1, t);
            stream1.Position = 0;
            var sr = new StreamReader(stream1);
            return (sr.ReadToEnd());
        }
    }
}
