using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Utilities;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiSelectMap : GuiComponent
    {
        private readonly ButtonContainer btnNextMap;
        private readonly ButtonContainer btnPreviousMap;
        private readonly ButtonContainer btnStartGame;

        private readonly Dictionary<string, Map> mapList;

        private int currentMap;

        public GuiSelectMap()
        {
            mapList = ResourceLoader.LoadAllMaps();
            btnPreviousMap = GuiLobbyMenu.CreateButton("Previous Map", new Rectangle(
                StaticEngine.RenderWidth / 4 - 100, (int)(StaticEngine.RenderHeight / 1.5) - 30, 200, 60
            ));

            btnPreviousMap.ButtonClicked += () =>
            {
                currentMap--;
                if (currentMap < 0)
                {
                    currentMap = mapList.Count - 1;
                }
            };

            btnStartGame = GuiLobbyMenu.CreateButton("Start Game", new Rectangle(
                StaticEngine.RenderWidth / 2 - 100, (int)(StaticEngine.RenderHeight / 1.5) - 30, 200, 60
            ));
            btnStartGame.ButtonClicked += () => Program.LoadGame(mapList.Keys.ElementAt(currentMap));

            btnNextMap = GuiLobbyMenu.CreateButton("Next Map", new Rectangle(
                StaticEngine.RenderWidth / 2 + 200, (int)(StaticEngine.RenderHeight / 1.5) - 30, 200, 60
            ));

            btnNextMap.ButtonClicked += () =>
            {
                currentMap++;
                if (currentMap >= mapList.Count)
                {
                    currentMap = 0;
                }
            };
        }

        public override void Dispose()
        {
            lock(mapList)
            {
                foreach (Map map in mapList.Values)
                {
                    map.Dispose();
                }
                mapList.Clear();
            }
        }

        public override void Render(Graphics g)
        {
            lock(mapList)
            {
                // Zeichne Karte
                g.DrawImage(mapList.Values.ElementAt(currentMap).BackgroundImage,
                    0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight
                );
            }

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

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Program.LoadGame(null);
            }
        }

    }
}
