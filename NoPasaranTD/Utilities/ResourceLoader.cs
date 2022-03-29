using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NoPasaranTD.Utilities
{
    public static class ResourceLoader
    {
        /// <summary>
        /// Lädt eine eingebettete Ressource und interpretiert sie als Bitmap
        /// </summary>
        /// <param name="name">Name der Ressource</param>
        /// <returns>Eine Bitmap von der eingebetteten Ressource</returns>
        public static Bitmap LoadBitmapResource(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                return new Bitmap(stream);
            }
        }

        /// <summary>
        /// Sucht nach Maps im jeweiligen Verzeichnis und lädt diese
        /// </summary>
        /// <returns>Ein Dictionary was den Mapnamen und die Mapinstanz für jeden Eintrag speichert</returns>
        public static Dictionary<string, Map> LoadAllMaps()
        {
            string path = "NoPasaranTD.Resources.Maps.";
            Assembly assembly = Assembly.GetExecutingAssembly();

            string[] names = assembly.GetManifestResourceNames()
                .Where(x => x.StartsWith(path) && x.EndsWith(".json")).ToArray();

            Dictionary<string, Map> maps = new Dictionary<string, Map>();
            for (int i = 0; i < names.Length; i++)
            {
                string mapname = names[i].Substring(path.Length, names[i].Length - path.Length - ".json".Length);
                maps[mapname] = MapData.GetMapByFileName(mapname);
            }
            return maps;
        }

        /// <summary>
        /// Lädt alle Zitate aus der Dichter und Denker Datei
        /// </summary>
        /// <returns></returns>
        public static List<string> DichterUndDenker()
        {
            string rawData = "";
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("NoPasaranTD.Resources.dichter_und_denker.txt"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    rawData = reader.ReadToEnd();
                }
            }

            return new List<string>(rawData.Split('\n'));
        }

        public static List<Image> LoadMemes()
        {
            List<Image> images = new List<Image>();

            for (int i = 1; i <= 18; i++)
            {
                images.Add(LoadBitmapResource("NoPasaranTD.Resources.Meme.meme_TD_" + i + ".jpg"));
            }

            return images;
        }
    }
}
