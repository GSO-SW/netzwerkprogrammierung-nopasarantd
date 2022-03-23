using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Visuals.Main;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class GuiSelectMap : GuiComponent
    {
        private readonly ButtonContainer btnNextMap;
        private readonly ButtonContainer btnPreviousMap;
        private readonly ButtonContainer btnStartGame;

        public Dictionary<Map, string> mapList;
        
        public int CurrentMap;


        public GuiSelectMap()
        {
            GetMaps();



            btnNextMap = GuiMainMenu.CreateButton("Next Map", new Rectangle(
                StaticEngine.RenderWidth / 2 + 200, (int)(StaticEngine.RenderHeight / 1.5) - 30, 200, 60
            ));

            btnPreviousMap = GuiMainMenu.CreateButton("Previous Map", new Rectangle(
                StaticEngine.RenderWidth / 4 - 100, (int)(StaticEngine.RenderHeight / 1.5) - 30, 200, 60
            ));

            btnStartGame = GuiMainMenu.CreateButton("Start Game", new Rectangle(
                StaticEngine.RenderWidth / 2 - 100, (int)(StaticEngine.RenderHeight / 1.5) - 30, 200, 60
            ));
            btnNextMap.ButtonClicked += () =>
            {
                CurrentMap=++CurrentMap%mapList.Count;
            };

            btnPreviousMap.ButtonClicked += () =>
            {

                CurrentMap = Math.Abs(--CurrentMap % mapList.Count);
            };


            btnStartGame.ButtonClicked += () => Program.LoadGame(mapList.Values.ElementAt(CurrentMap));



        }

        public void GetMaps()
        {
            string Ressourcepath = "NoPasaranTD.Resources.Maps.";
            Assembly assembly = Assembly.GetExecutingAssembly();

            string[] Ressourcenames = assembly.GetManifestResourceNames().Where(x => x.StartsWith(Ressourcepath) && x.EndsWith(".json")).ToArray();
            mapList = new Dictionary<Map, string>();

            foreach (var item in Ressourcenames)
            {
                string Mapname = item.Substring(Ressourcepath.Length, item.Length - Ressourcepath.Length - ".json".Length);

                mapList.Add(MapData.GetMapByFileName(Mapname), Mapname);

            }
            CurrentMap = 0;

        }

        public override void Render(Graphics g)
        {


            
                float scaledWidth = (float)StaticEngine.RenderWidth / mapList.Keys.ElementAt(CurrentMap).BackgroundImage.Width;
                float scaledHeight = (float)StaticEngine.RenderHeight / mapList.Keys.ElementAt(CurrentMap).BackgroundImage.Height;

                Matrix m = g.Transform;
                g.ScaleTransform(scaledWidth, scaledHeight);
                g.DrawImageUnscaled(mapList.Keys.ElementAt(CurrentMap).BackgroundImage, 0, 0);
                g.Transform = m;
            
            btnNextMap.Render(g);
            btnPreviousMap.Render(g);
            btnStartGame.Render(g);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            btnNextMap.MouseDown(e);
            btnPreviousMap.MouseDown(e);
            btnStartGame.MouseDown(e);
        }

    }
}
