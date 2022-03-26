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
    }
}
