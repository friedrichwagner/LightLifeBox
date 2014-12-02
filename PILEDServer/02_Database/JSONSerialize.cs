using System;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace Lumitech.Helpers
{
    public static class Extensions
    {
        public static string ToJson<T>(this T obj)
        {
            MemoryStream stream = new MemoryStream();

            try
            {
                DataContractJsonSerializer jsSerializer = new DataContractJsonSerializer(typeof(T));
                jsSerializer.WriteObject(stream, obj);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }

        public static void ToFile<T>(this T obj, string fname)
        {
            using (StreamWriter f = new StreamWriter(fname))
            {
                f.Write(obj.ToJson());
            }
        }

        public static T FromJson<T>(this string input)
        {
            MemoryStream stream = new MemoryStream();

            try
            {
                DataContractJsonSerializer jsSerializer = new DataContractJsonSerializer(typeof(T));
                stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                T obj = (T)jsSerializer.ReadObject(stream);

                return obj;
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
        }

        public static T FromFile<T>(this string fname)
        {
            using (StreamReader f = new StreamReader(fname))
            {
                return FromJson<T>(f.ReadLine());
            }
        }
    }
}





