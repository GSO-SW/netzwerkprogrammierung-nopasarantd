using NoPasaranTD.Engine;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class GuiPauseMenu : GuiComponent
    {

        private static readonly Brush BACKGROUND_COLOR = new SolidBrush(Color.FromArgb(150, Color.Black));
        private static readonly StringFormat TEXT_FORMAT = new StringFormat()
        { // Zeichenformat für den Titelstring
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Far,
        };

        private readonly Game game;
        private readonly ButtonContainer btnBackToGame;
        private readonly ButtonContainer btnLeaveGame;
        public GuiPauseMenu(Game game)
        {
            this.game = game;
            btnBackToGame = CreateButton("Back to Game", new Rectangle(
                StaticEngine.RenderWidth / 2 - 150,
                StaticEngine.RenderHeight / 2 - 35,
                300, 35
            ));
            btnBackToGame.ButtonClicked += BackToGame;

            btnLeaveGame = CreateButton("Leave Game", new Rectangle(
                StaticEngine.RenderWidth / 2 - 150,
                StaticEngine.RenderHeight / 2 + 5,
                300, 35
            ));
            btnLeaveGame.ButtonClicked += () => Program.LoadGame(null); // Entlade das Spiel (Kehrt automatisch zum Hauptmenü zurück)
        }

        private void BackToGame()
        {
            Program.LoadScreen(null); // Entlade diesen Screen
            game.Paused = false; // Setze das Spiel fort
        }

        public override void Render(Graphics g)
        {
            // Verdunkelt das Spiel im Hintergrund
            g.FillRectangle(BACKGROUND_COLOR, 0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight);

            // Zeichne "Game Paused" über den "Back to Game"-Button
            g.DrawString("Game Paused", StandartHeader1Font, Brushes.White,
                StaticEngine.RenderWidth / 2,
                btnBackToGame.Bounds.Y - 5,
                TEXT_FORMAT
            );

            btnBackToGame.Render(g);
            btnLeaveGame.Render(g);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            btnBackToGame.MouseDown(e);
            btnLeaveGame.MouseDown(e);
        }

        private static ButtonContainer CreateButton(string text, Rectangle bounds)
        {
            return new ButtonContainer
            {
                Bounds = bounds,
                Content = text,
                StringFont = StandartText1Font,
                Foreground = Brushes.Black,
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Blue,
                Margin = 2
            };
        }

    }
}
