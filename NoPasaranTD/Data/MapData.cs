using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using NoPasaranTD.Model;

namespace NoPasaranTD.Data
{
    public class MapData
    {
        /// <summary>
        /// Übergiebt ein Map Objekt bei der Angabe dessen jeweiligen File-Pfades
        /// </summary>
        /// <param name="fullPath">Pfad des gespeicherten Map-Modells</param>
        /// <returns></returns>
        public async Task<Map> GetMapByPathAsync(string fullPath)
        {
            Map obj = null;
            using (StreamReader streamReader = new StreamReader(fullPath))
            {
                string rawData = await streamReader.ReadToEndAsync();
                try { obj = JsonConvert.DeserializeObject<Map>(rawData); }
                catch (Exception) { /* TODO: Exception  */ }               
            }     
            return obj;
        }

        /// <summary>
        /// Speichert ein Map Objekt.
        /// </summary>
        /// <param name="fileName">Name der neu zu erstellenden Datei</param>
        /// <param name="map">Die zur speichernde Map</param>
        public async Task CreateNewMapAsync(string fileName, Map map)
        {
            string savePath = Environment.CurrentDirectory;
            File.Create(savePath + "//" + fileName + ".json");

            using (StreamWriter streamWriter = new StreamWriter(savePath + "//" + fileName + ".json"))
                await streamWriter.WriteAsync(JsonConvert.SerializeObject(map, Formatting.Indented));
        }
    }
}
