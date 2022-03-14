using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame.GameOver
{
    public class GuiGameOver:GuiComponent
    {
        private ButtonContainer btnReturnLobby { get; }

        private SoundPlayer Youdied = new SoundPlayer(Environment.CurrentDirectory + "\\img\\Youdied.wav");
        
        Bitmap GameoverScreen = new Bitmap(Environment.CurrentDirectory+ "\\img\\YouDied2.jpg");
        public override void Render(Graphics g)
        {
            g.DrawImage(GameoverScreen, 0, 0);
            btnReturnLobby.Render(g);
        }

        public GuiGameOver() 
        {
            Youdied.Play();
            btnReturnLobby =  new ButtonContainer
            {
                Bounds = new Rectangle(StaticEngine.RenderWidth/2-100, StaticEngine.RenderHeight/ 2+80, 200,60),
                Content = "Return to Lobby",
                StringFont = StandartText1Font,
                Foreground = Brushes.Red,
                Background = Brushes.Black,
                BorderBrush = Brushes.Blue,
                Margin = 2
            };
            btnReturnLobby.ButtonClicked += () => Program.LoadScreen(new Main.GuiLobby());
        }
        public override void MouseDown(MouseEventArgs e)
        {
            btnReturnLobby.MouseDown(e);
        }

    }
}
