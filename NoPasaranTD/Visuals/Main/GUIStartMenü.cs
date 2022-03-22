using NoPasaranTD.Engine;
using NoPasaranTD.Visuals.Ingame;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GUIStartMenü : GuiComponent
    {
        // Führt zu einem Singleplayer Game
        private ButtonContainer singleplayerButton;

        // Führt zu einem Multiplayer Game
        private ButtonContainer multiplayerButton;

        private bool initialized = false;
        private Game currentGame;

        public GUIStartMenü()
        {
              
        }

        public override void Render(Graphics g)
        {
            if (!initialized || !Visible) return;

            string title = "No Pasaran!";
            Size titleSize = TextRenderer.MeasureText(title, StandartHeader1Font);

            g.DrawString(title, StandartHeader1Font, Brushes.Black, StaticEngine.RenderWidth - titleSize.Width / 2, 50);

            singleplayerButton.Render(g);
            multiplayerButton.Render(g);
        }
        public override void Update()
        {
            if (!initialized || !Visible) return;
            singleplayerButton.Update();
            multiplayerButton.Update();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (!initialized) return;
            singleplayerButton.MouseDown(e);
            multiplayerButton.MouseDown(e);
        }

        public void Init(Game game)
        {
            currentGame = game;
            singleplayerButton = new ButtonContainer()
            {
                Content = "Singleplayer",
                Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
                BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
                Margin = 1,
                StringFont = StandartHeader2Font,
                Bounds = new Rectangle(StaticEngine.RenderWidth / 2 + 100, 50, 200, StaticEngine.RenderHeight / 2 - 60),
            };

            multiplayerButton = new ButtonContainer()
            {
                Content = "Singleplayer",
                Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
                BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
                Margin = 1,
                StringFont = StandartHeader2Font,
                Bounds = new Rectangle(StaticEngine.RenderWidth / 2 - 100, 50, 200, StaticEngine.RenderHeight / 2 + 60),
            };

            singleplayerButton.ButtonClicked += SingleplayerButton_ButtonClicked;
            multiplayerButton.ButtonClicked += MultiplayerButton_ButtonClicked;

            initialized = true;
        }

        private void MultiplayerButton_ButtonClicked()
        {
            Program.LoadScreen(new GUIStartMenü());
        }

        private void SingleplayerButton_ButtonClicked()
        {
            
        }
    }
}
