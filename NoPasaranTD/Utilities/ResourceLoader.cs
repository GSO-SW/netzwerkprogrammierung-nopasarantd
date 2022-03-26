using NoPasaranTD.Data;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Utilities
{
    public static class ResourceLoader
    {
        public static Bitmap LoadBitmapResource(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(name))
            {
                return new Bitmap(stream);
            }
        }
        public static Dictionary<string, Map> LoadAllMaps()
        {
            string Ressourcepath = "NoPasaranTD.Resources.Maps.";
            Assembly assembly = Assembly.GetExecutingAssembly();

            string[] Resourcenames = assembly.GetManifestResourceNames().Where(x => x.StartsWith(Ressourcepath) && x.EndsWith(".json")).ToArray();
            Dictionary<string, Map> maps = new Dictionary<string, Map>();

            for (int i = 0; i < Resourcenames.Length; i++)
            {
                string Mapname = Resourcenames[i].Substring(Ressourcepath.Length, Resourcenames[i].Length - Ressourcepath.Length - ".json".Length);

                maps[Mapname] = MapData.GetMapByFileName(Mapname);

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
                    rawData = reader.ReadToEnd();
            }

            return new List<string>(rawData.Split('\n'));
        }

        public static List<Image> LoadMemes()
        {
            List<Image> images = new List<Image>();

            for (int i = 1; i <= 8; i++)
                images.Add(LoadBitmapResource("NoPasaranTD.Resources.Meme.meme_TD_" + i + ".jpg"));

            return images;
        }
    }
}
