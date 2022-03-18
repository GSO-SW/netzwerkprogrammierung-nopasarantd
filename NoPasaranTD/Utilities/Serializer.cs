using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NoPasaranTD.Utilities
{
    public static class Serializer
    {
        /// <summary>
        /// Serialisiert ein Objekt zu einem byte[]
        /// </summary>
        /// <param name="obj">Objekt was zu serialisieren gilt</param>
        /// <returns>Daten als byte[] des objektes</returns>
        public static byte[] SerializeObject(object obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (var memorys = new MemoryStream()) 
            {
                formatter.Serialize(memorys, obj);  
                return memorys.ToArray();   
            }
        }

        /// <summary>
        /// Deserialisiert ein byte[] zu einem Objekt
        /// </summary>
        /// <param name="obj">Die Daten des Objektes als byte[]</param>
        /// <returns>Ein deserialisiertes Objekt aus byte[]</returns>
        public static object DeserializeObject(byte[] obj)
        {
            MemoryStream ms = new MemoryStream();   
            BinaryFormatter formatter = new BinaryFormatter();

            ms.Write(obj, 0, obj.Length);
            ms.Seek(0, SeekOrigin.Begin);   

            return formatter.Deserialize(ms);  
        }
    }
}
