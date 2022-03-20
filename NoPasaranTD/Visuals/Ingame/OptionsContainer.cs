using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    public class OptionsContainer : GuiComponent
    {
        public Brush Foreground { get; set; }
        public Brush Background { get; set; }

        private Game currentGame;
        private int buttonMargin = 5;

        private bool isExpanded = true;
        private bool isStartVisible = false;

        private readonly ButtonContainer startButton = new ButtonContainer()
        {
            Content = "▶",
            Background = new SolidBrush(Color.FromArgb(122, 127, 255)),
            BorderBrush = new SolidBrush(Color.FromArgb(72, 75, 171)),
            Margin = 2,
            StringFont = StandartText1Font,
        };

        private readonly ButtonContainer autoStartButton = new ButtonContainer()
        {
            Content = "⤻",
            Background = new SolidBrush(Color.FromArgb(122, 127, 255)),
            BorderBrush = new SolidBrush(Color.FromArgb(72, 75, 171)),
            Margin = 2,
            StringFont = StandartText1Font,
        };

        private readonly ButtonContainer playersButton = new ButtonContainer()
        {
            Content = "≡",
            Background = new SolidBrush(Color.FromArgb(122, 127, 255)),
            BorderBrush = new SolidBrush(Color.FromArgb(72, 75, 171)),
            Margin = 2,
            StringFont = StandartText1Font,          
        };

        private readonly ButtonContainer expandButton = new ButtonContainer()
        {
            Content = "⬅",
            Background = new SolidBrush(Color.FromArgb(122, 127, 255)),
            BorderBrush = new SolidBrush(Color.FromArgb(72, 75, 171)),
            Margin = 2,
            StringFont = StandartText1Font,
        };

        public override void Render(Graphics g)
        {
            g.FillRectangle(Background, Bounds);

            if (currentGame.WaveManager.IsCompleted && isExpanded)
            {
                startButton.Render(g);
            }
            if (isExpanded)
            {
                autoStartButton.Render(g);
                playersButton.Render(g);
            }                        
            expandButton.Render(g);
        }

        public override void Update()
        {
            autoStartButton.Update();
            playersButton.Update();
            expandButton.Update();
            startButton.Update();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            autoStartButton.MouseDown(e);
            playersButton.MouseDown(e);
            expandButton.MouseDown(e);
            startButton.MouseDown(e);
        }

        public void Init(Game gamer)
        {
            int buttonWidth = (Bounds.Width - 6 * buttonMargin)/4;
            int buttonHeight = Bounds.Height - buttonMargin*2;

            currentGame = gamer;

            startButton.Bounds = new Rectangle(Bounds.X + buttonMargin, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            autoStartButton.Bounds = new Rectangle(Bounds.X + buttonMargin * 2 + buttonWidth, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            playersButton.Bounds = new Rectangle(Bounds.X + buttonMargin * 3 + buttonWidth * 2, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            expandButton.Bounds = new Rectangle(Bounds.X + buttonMargin * 4 + buttonWidth * 3, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);

            startButton.Foreground = Foreground;
            autoStartButton.Foreground = Foreground;
            playersButton.Foreground = Foreground;
            expandButton.Foreground = Foreground;

            startButton.ButtonClicked += StartButton_ButtonClicked;
            autoStartButton.ButtonClicked += AutoStartButton_ButtonClicked;
            playersButton.ButtonClicked += PlayersButton_ButtonClicked; ;
            expandButton.ButtonClicked += ExpandButton_ButtonClicked;
        }

        private void PlayersButton_ButtonClicked()
        {
            currentGame.UILayout.PlayerListContainer.Visible = !currentGame.UILayout.PlayerListContainer.Visible;
        }

        private async void ExpandButton_ButtonClicked()
        {
            isExpanded = !isExpanded;
            if (isExpanded)
                await ExpandTo(expandButton.Bounds.Width * 4 + buttonMargin * 6);
            else
                await CollapseTo(expandButton.Bounds.Width + buttonMargin*6);
        }

        private void AutoStartButton_ButtonClicked()
        {
            if (currentGame.WaveManager.AutoStart)
                autoStartButton.Content = "⤻";
            else
                autoStartButton.Content = "⭯";

            currentGame.WaveManager.AutoStart = !currentGame.WaveManager.AutoStart;
        }

        private void StartButton_ButtonClicked()
        {
            currentGame.WaveManager.StartSpawn();
        }

        public async Task ExpandTo(int aimSize)
        {
            while (Bounds.Width <= aimSize)
            {
                Bounds= new Rectangle(Bounds.X -10,Bounds.Y,Bounds.Width+20,Bounds.Height);
                expandButton.Bounds = new Rectangle(expandButton.Bounds.X + 10, expandButton.Bounds.Y, expandButton.Bounds.Width, expandButton.Bounds.Height);
                await Task.Delay(5);
            }
        }

        public async Task CollapseTo(int aimSize)
        {
            int startPos = Bounds.X;
            while (Bounds.Width >= aimSize)
            {
                Bounds = new Rectangle(Bounds.X + 10, Bounds.Y, Bounds.Width - 20, Bounds.Height);
                expandButton.Bounds = new Rectangle(expandButton.Bounds.X - 10, expandButton.Bounds.Y, expandButton.Bounds.Width, expandButton.Bounds.Height);
                await Task.Delay(5);
            }
        }
    }
}
