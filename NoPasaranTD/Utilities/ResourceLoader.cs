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
    }
}
