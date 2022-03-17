using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame.GameOver
{
    public class GuiGameOver:GuiComponent
    {
        private readonly ButtonContainer btnReturnLobby;

        private readonly SoundPlayer Youdied;
        
        private readonly Bitmap  GameoverScreen;
        
        public GuiGameOver() 
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("NoPasaranTD.Resources.gameoverscreen.jpg")) 
            { //Gameoverscreen wird aus Resourceordner geladen 
                GameoverScreen = new Bitmap(stream);
            }
         
            //Gameoversound wird aus resourceordner geladen
            Youdied = new SoundPlayer(assembly.GetManifestResourceStream("NoPasaranTD.Resources.gameoversound.wav"));
            Youdied.Play(); // TODO: Stoppen und Schließen des Streams beim verlassen

            btnReturnLobby =  new ButtonContainer
            {
                Bounds = new Rectangle(StaticEngine.RenderWidth/2-100, (int)(StaticEngine.RenderHeight/ 1.5)-30, 200,60),
                Content = "Return to Lobby",
                StringFont = StandartText1Font,
                Foreground = Brushes.Red,
                Background = Brushes.Black,
                BorderBrush = Brushes.Blue,
                Margin = 2
            };
            btnReturnLobby.ButtonClicked += () => Program.LoadScreen(new Main.GuiMainMenu());
        }
        
        public override void Render(Graphics g)
        {
            { // Zeichne Karte
                float scaledWidth = (float)StaticEngine.RenderWidth / GameoverScreen.Width;
                float scaledHeight = (float)StaticEngine.RenderHeight / GameoverScreen.Height;

                Matrix m = g.Transform;
                g.ScaleTransform(scaledWidth, scaledHeight);
                g.DrawImageUnscaled(GameoverScreen, 0, 0);
                g.Transform = m;
            }
         
            btnReturnLobby.Render(g);
        }
        
        public override void MouseDown(MouseEventArgs e)
        {
            btnReturnLobby.MouseDown(e);
        }

    }
}
