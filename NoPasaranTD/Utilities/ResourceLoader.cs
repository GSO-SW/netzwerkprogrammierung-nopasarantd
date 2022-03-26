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
    }
}
