using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using NoPasaranTD.Model;

namespace NoPasaranTD.Data
{
    /// <summary>
    /// Tools für die Daten der Maps
    /// </summary>
    public class MapData
    {
        /// <summary>
        /// Übergiebt ein Map Objekt bei der Angabe dessen jeweiligen File-Pfades
        /// </summary>
        /// <param name="fullPath">Pfad des gespeicherten Map-Modells</param>
        /// <returns></returns>
        public async Task<Map> GetMapByPathAsync(string fileName)
        {
            object obj = null;      
            string savePath = Environment.CurrentDirectory + "\\" + fileName + ".json";

            if (!File.Exists(savePath))
                throw new FileNotFoundException(savePath);

            using (StreamReader streamReader = new StreamReader(savePath))
            {           
                string rawData = await streamReader.ReadToEndAsync();
                try { obj = JsonConvert.DeserializeObject<Map>(rawData); }
                catch (Exception) { /* TODO: Exception  */ }
                streamReader.Close();
            }     
            return obj as Map;
        }

        /// <summary>
        /// Speichert ein Map Objekt.
        /// </summary>
        /// <param name="fileName">Name der neu zu erstellenden Datei</param>
        /// <param name="map">Die zur speichernde Map</param>
        public async Task CreateNewMapAsync(string fileName, Map map)
        {
            string savePath = Environment.CurrentDirectory + "\\" + fileName + ".json";

            if (!File.Exists(savePath))
                File.Create(savePath);

            using (StreamWriter streamWriter = new StreamWriter(savePath))
            {
                await streamWriter.WriteAsync(JsonConvert.SerializeObject(map, Formatting.Indented));
                streamWriter.Close();
            }                
        }
    }  
}
