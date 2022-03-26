using Newtonsoft.Json;
using NoPasaranTD.Model;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace NoPasaranTD.Data
{
    /// <summary>
    /// Tools für das Arbeiten mit den Daten der Maps
    /// </summary>
    public class MapData
    {
        /// <summary>
        /// Übergibt ein Map Objekt mithilfe von Angabe eines Dateipfades.</br>
        /// Die Methode erlaubt keine parallelen zugriffe und läuft synchron!
        /// </summary>
        /// <param name="fullPath">Pfad des gespeicherten Map-Modells</param>
        /// <returns>Die deserialisierte Map</returns>
        /// <exception cref="Exception">Wenn die Datei fehlerhaft ist</exception>
        /// <exception cref="FileNotFoundException">Wenn die Datei nicht gefunden wird</exception>
        public static Map GetMapByFileName(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "NoPasaranTD.Resources.Maps." + fileName + ".json";

            Map obj;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader streamReader = new StreamReader(stream))
            {
                string rawData = streamReader.ReadToEnd();
                try { obj = GetMapFromJson(JsonConvert.DeserializeObject(rawData)); }
                catch (Exception ex) { throw new Exception("Failed loading Map", ex); }

                streamReader.Close();
            }
            return obj;
        }

        /// <summary>
        /// Übergibt ein Map Objekt mithilfe von Angabe eines Dateipfades.</br>
        /// Die Methode erlaubt keine parallelen zugriffe und läuft synchron!
        /// </summary>
        /// <param name="fullPath">Pfad des gespeicherten Map-Modells</param>
        /// <returns>Die deserialisierte Map</returns>
        /// <exception cref="Exception">Wenn die Datei fehlerhaft ist</exception>
        /// <exception cref="FileNotFoundException">Wenn die Datei nicht gefunden wird</exception>
        public static async Task<Map> GetMapByFileNameAsync(string fileName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "NoPasaranTD.Resources.Maps." + fileName + ".json";

            Map obj;
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader streamReader = new StreamReader(stream))
            {
                string rawData = await streamReader.ReadToEndAsync();
                try { obj = GetMapFromJson(JsonConvert.DeserializeObject(rawData)); }
                catch (Exception ex) { throw new Exception("Failed loading Map", ex); }

                streamReader.Close();
            }
            return obj;
        }

        /// <summary>
        /// Speichert ein Map Objekt asynchron.</br>
        /// Die Methode erlaubt keine parallele zugriffe!
        /// </summary>
        /// <param name="fileName">Name der neu zu erstellenden Datei</param>
        /// <param name="map">Die zur speichernde Map</param>
        public static async Task SaveMapAsync(Map map, string fileName)
        {
            string savePath = Environment.CurrentDirectory + "\\" + fileName + ".json";

            FileStream fileStream;
            // Prüft ob die Datei bereits existiert
            if (!File.Exists(savePath))
            {
                fileStream = File.Create(savePath);
            }
            else
            {
                fileStream = new FileStream(savePath, FileMode.Open, FileAccess.ReadWrite);
            }

            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                await streamWriter.WriteAsync(JsonConvert.SerializeObject(map, Formatting.Indented));
                streamWriter.Close();
            }
        }

        /// <summary>
        /// Erstellt aus einem JObject dynamisch eine Map
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns>Die ausgelesene Map</returns>
        private static Map GetMapFromJson(object rawData)
        {
            // Dynamisches Objekt dataMap
            dynamic dataMap = rawData;

            // Instanziert ein neues Map-Objekt
            Map mapObj = new Map()
            {
                Name = dataMap.Name,
                Dimension = dataMap.Dimension,
                Obstacles = new List<Obstacle>(),
                PathWidth = dataMap.PathWidth,
                BackgroundPath = dataMap.BackgroundPath,
                BalloonPath = new Vector2D[dataMap.BalloonPath.Count]
            };

            // Erstellt alle Obstacle-Objekte aus dem JObject Objekt
            for (int i = 0; i < dataMap.Obstacles.Count; i++)
            {
                ObstacleType obstacleType = dataMap.Obstacles[i].ObstacleType;
                Rectangle rectangle = dataMap.Obstacles[i].Hitbox;

                mapObj.Obstacles.Add(new Obstacle(obstacleType, rectangle));
            }

            // Erstellt alle Vector-Objekte aus dem JObject Objekt
            for (int i = 0; i < dataMap.BalloonPath.Count; i++)
            {
                float x = dataMap.BalloonPath[i].X;
                float y = dataMap.BalloonPath[i].Y;
                mapObj.BalloonPath[i] = new Vector2D(x, y);
            }
            return mapObj;
        }
    }
}
