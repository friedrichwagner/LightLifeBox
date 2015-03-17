using System;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic; 

namespace Lumitech.Helpers
{
    public class MyDictionary : Dictionary<string, string>
    {
        public MyDictionary(Dictionary<string, string> d)
        {
            foreach (KeyValuePair<string, string> pair in d)
            {
                Add(pair.Key, pair.Value);
            }
        }

        public int TryGetIntValue(string key, out int newI)
        {
            newI = 0;
            try
            {
                string s;
                if (TryGetValue(key, out s))
                {
                    newI=Int32.Parse(s);
                }
            }
            catch{}

            return newI;
        }

        public byte TryGetIntValue(string key, out byte newI)
        {
            newI = 0;
            try
            {
                string s;
                if (TryGetValue(key, out s))
                {
                    newI = byte.Parse(s);
                }
            }
            catch { }

            return newI;
        }

        public double TryGetDoubleValue(string key, out double newD)
        {
            newD = 0.0;
            try
            {
                string s;
                if (TryGetValue(key, out s))
                {
                    newD = Double.Parse(s);
                }
            }
            catch { }

            return newD;
        }

        public float TryGetDoubleValue(string key, out float newD)
        {
            newD = 0.0f;
            try
            {
                string s;
                if (TryGetValue(key, out s))
                {
                    newD = float.Parse(s);
                }
            }
            catch { }

            return newD;
        }
    }

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





