using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using Newtonsoft.Json;
using NoPasaranTD.Model;

namespace NoPasaranTD.Data
{
    /// <summary>
    /// Tools für das Arbeiten mit den Daten der Maps
    /// </summary>
    public class MapData
    {
        /// <summary>
        /// Übergiebt ein Map Objekt bei der Angabe dessen jeweiligen File-Pfades.</br>
        /// Die Methode erlaubt keine parallele zugriffe und läuft synchron!
        /// </summary>       
        /// <param name="fullPath">Pfad des gespeicherten Map-Modells</param>
        /// <returns></returns>
        /// <exception cref="Exception">Wenn Datei fehlerhaft ist</exception>
        /// <exception cref="FileNotFoundException">Wenn Datei nicht gefunden wird</exception>
        public static Map GetMapByFile(string fileName)
        {
            Map obj = null;
            string savePath = Environment.CurrentDirectory + "\\" + fileName + ".json";

            // Prüft ob die Datei bereits existiert
            if (!File.Exists(savePath))
                throw new FileNotFoundException(savePath);

            using (StreamReader streamReader = new StreamReader(savePath))
            {
                string rawData = streamReader.ReadToEnd();

                try { obj = GetMapFromJson(JsonConvert.DeserializeObject(rawData)); }
                catch (Exception) { throw new Exception("Failed loading Map"); }

                streamReader.Close();
            }
            return obj;
        }

        /// <summary>
        /// Übergiebt ein Map Objekt bei der Angabe dessen jeweiligen File-Pfades.</br>
        /// Die Methode erlaubt keine parallele zugriffe und läuft asynchron!
        /// </summary>       
        /// <param name="fullPath">Pfad des gespeicherten Map-Modells</param>
        /// <returns></returns>
        /// <exception cref="Exception">Wenn Datei fehlerhaft ist</exception>
        /// <exception cref="FileNotFoundException">Wenn Datei nicht gefunden wird</exception>
        public static async Task<Map> GetMapByFileAsync(string fileName)
        {
            Map obj = null;      
            string savePath = Environment.CurrentDirectory + "\\" + fileName + ".json";

            // Prüft ob die Datei bereits existiert
            if (!File.Exists(savePath))
                throw new FileNotFoundException(savePath);

            using (StreamReader streamReader = new StreamReader(savePath))
            {           
                string rawData = await streamReader.ReadToEndAsync();

                try { obj = GetMapFromJson(JsonConvert.DeserializeObject(rawData)); }
                catch (Exception) { throw new Exception("Failed loading Map"); }

                streamReader.Close();
            }
            return obj;
        }

        /// <summary>
        /// Speichert ein Map Objekt.</br>
        /// Die Methode erlaubt keine parallele zugriffe !
        /// </summary>
        /// <param name="fileName">Name der neu zu erstellenden Datei</param>
        /// <param name="map">Die zur speichernde Map</param>
        public static async Task CreateNewMapAsync(string fileName, Map map)
        {
            string savePath = Environment.CurrentDirectory + "\\" + fileName + ".json";

            FileStream fileStream = null;

            // Prüft ob die Datei bereits existiert
            if (!File.Exists(savePath))
                fileStream = File.Create(savePath);
            else
                fileStream = new FileStream(savePath, FileMode.Open, FileAccess.ReadWrite);

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
                Obstacles = new List<Obstacle>(),
                BackgroundPath = dataMap.BackgroundPath,
                BalloonPath = new Utilities.Vector2D[dataMap.BalloonPath.Count]
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

                mapObj.BalloonPath[i] = new Utilities.Vector2D(x,y);
            }               
            return mapObj;
        }
    }  
}
