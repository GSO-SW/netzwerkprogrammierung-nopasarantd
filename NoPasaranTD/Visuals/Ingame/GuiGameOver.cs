using NoPasaranTD.Engine;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame.GameOver
{
    public class GuiGameOver : GuiComponent
    {
        private readonly ButtonContainer btnReturnLobby;

        private readonly Bitmap GameOverScreen;
        private readonly SoundPlayer GameOverSound;

        public GuiGameOver()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("NoPasaranTD.Resources.gameoverscreen.jpg"))
            { // GameOverScreen wird aus Resourceordner geladen 
                GameOverScreen = new Bitmap(stream);
            }

            //Gameoversound wird aus resourceordner geladen
            GameOverSound = new SoundPlayer(assembly.GetManifestResourceStream("NoPasaranTD.Resources.gameoversound.wav"));

            btnReturnLobby = new ButtonContainer
            {
                Bounds = new Rectangle(StaticEngine.RenderWidth / 2 - 100, (int)(StaticEngine.RenderHeight / 1.5) - 30, 200, 60),
                Content = "Return to Lobby",
                StringFont = StandartText1Font,
                Foreground = Brushes.Red,
                Background = Brushes.Black,
                BorderBrush = Brushes.Blue,
                Margin = 2
            };
            btnReturnLobby.ButtonClicked += () => Program.LoadGame(null); // Entlade Spiel (Wirft einen zurück ins Hauptmenü)

            GameOverSound.Play();
        }

        public override void Dispose()
        {
            GameOverScreen?.Dispose();
            GameOverSound?.Dispose();
        }

        public override void Render(Graphics g)
        {
            // Zeichne GameOverScreen
            g.DrawImage(GameOverScreen,
                0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight
            );
            btnReturnLobby.Render(g);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            btnReturnLobby.MouseDown(e);
        }
    }
}
