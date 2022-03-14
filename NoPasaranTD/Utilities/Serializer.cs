using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Utilities
{
    public static class Serializer
    {
        public static byte[] EncodeObject(object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (var memorys = new MemoryStream()) 
            {
                formatter.Serialize(memorys, obj);  
                return memorys.ToArray();   
            }
        }
        public static object DecodeObject(byte[] obj)
        {
            MemoryStream ms = new MemoryStream();   
            BinaryFormatter formatter = new BinaryFormatter();

            ms.Write(obj, 0, obj.Length);
            ms.Seek(0, SeekOrigin.Begin);   

            return formatter.Deserialize(ms);  
        }
    }
}
