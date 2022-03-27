using NoPasaranTD.Engine;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Ingame
{
    /// <summary>
    /// Container welcher Optionen zum Game anzeigt und steuert
    /// </summary>
    public class OptionsContainer : GuiComponent
    {
        #region Properties

        /// <summary>
        /// Die Schriftfarbe der Komponenten
        /// </summary>
        public Brush Foreground { get; set; }

        /// <summary>
        /// Die Hintergrundfarbe des Controls
        /// </summary>
        public Brush Background { get; set; }

        #endregion

        #region Fields

        // Die genutzte Gameinstanz
        private Game currentGame;
        // Der Abstand zwischen den Steuerelementen
        public int ButtonMargin = 5;

        // Ist das Control ein- oder ausgeklappt
        public bool IsExpanded = true;

        #endregion
        #region GUI Components

        // Button zum Starten der nächsten Runde
        private readonly ButtonContainer startButton = new ButtonContainer()
        {
            Content = "▶",
            Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
            Margin = 1,
            StringFont = StandartHeader2Font,
        };

        // Button zum Steuern der Spielgeschwindigkeit
        private readonly ButtonContainer accelerationButton = new ButtonContainer()
        {
            Content = "►►",
            Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
            Margin = 1,
            StringFont = StandartText2Font,
        };

        // Button zum Steuern des Autostart Modes
        private readonly ButtonContainer autoStartButton = new ButtonContainer()
        {
            Content = "⤻",
            Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
            Margin = 1,
            StringFont = StandartHeader2Font,
        };

        // Button zum Öffnen der Spielerliste
        private readonly ButtonContainer playersButton = new ButtonContainer()
        {
            Content = "≡",
            Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
            Margin = 1,
            StringFont = StandartHeader2Font,
        };

        // Button zum ein- ausklappen des Menüs
        public readonly ButtonContainer expandButton = new ButtonContainer()
        {
            Content = "⬅",
            Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
            Margin = 1,
            StringFont = StandartHeader2Font,
        };

        // Button zum öffnen des Chates
        private readonly ButtonContainer chatButton = new ButtonContainer()
        {
            Content = "✉",
            Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
            BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
            Margin = 1,
            StringFont = StandartHeader2Font,
        };

        #endregion
        #region Override Methoden

        public override void Render(Graphics g)
        {
            if (!Visible)
            {
                return;
            }

            g.FillRectangle(Background, Bounds);

            // Zeigt den Startbutton nur wenn die Runde pausuert ist
            if (currentGame.WaveManager.IsRoundCompleted && IsExpanded)
            {
                startButton.Render(g);
            }

            // Zeigt die anderen Buttons nur wenn das Menu expandiert ist
            if (IsExpanded)
            {
                autoStartButton.Render(g);
                playersButton.Render(g);
                accelerationButton.Render(g);
                chatButton.Render(g);
            }

            // Zeigt den ein- ausklappen Button ummer
            expandButton.Render(g);
        }

        public override void Update()
        {
            accelerationButton.Content = "►►\n " + StaticEngine.TickAcceleration + "x";

            if (currentGame.WaveManager.AutoStart)
            {
                autoStartButton.Content = "⤻";
            }
            else
            {
                autoStartButton.Content = "⭯";
            }

            if (IsExpanded)
            {
                autoStartButton.Update();
                playersButton.Update();
                accelerationButton.Update();
                startButton.Update();
                chatButton.Update();
            }
            expandButton.Update();
        }

        public override void MouseDown(MouseEventArgs e)
        {
            if (IsExpanded)
            {
                autoStartButton.MouseDown(e);
                playersButton.MouseDown(e);
                startButton.MouseDown(e);
                accelerationButton.MouseDown(e);
                chatButton.MouseDown(e);
            }
            expandButton.MouseDown(e);
        }

        #endregion
        #region Init

        /// <summary>
        /// Initialisiert alle Komponenten
        /// </summary>
        /// <param name="game">Eine Spielinstanz in der diese Komponente laufen soll</param>
        public void Init(Game game)
        {
            int buttonWidth = (Bounds.Width - 7 * ButtonMargin) / 6;
            int buttonHeight = Bounds.Height - ButtonMargin * 2;

            currentGame = game;

            // Initialisiert die Grenzen der Buttons
            startButton.Bounds = new Rectangle(Bounds.X + ButtonMargin, Bounds.Y + ButtonMargin, buttonWidth, buttonHeight);
            accelerationButton.Bounds = new Rectangle(Bounds.X + ButtonMargin * 2 + buttonWidth, Bounds.Y + ButtonMargin, buttonWidth, buttonHeight);
            autoStartButton.Bounds = new Rectangle(Bounds.X + ButtonMargin * 3 + buttonWidth * 2, Bounds.Y + ButtonMargin, buttonWidth, buttonHeight);
            playersButton.Bounds = new Rectangle(Bounds.X + ButtonMargin * 4 + buttonWidth * 3, Bounds.Y + ButtonMargin, buttonWidth, buttonHeight);
            chatButton.Bounds = new Rectangle(Bounds.X + ButtonMargin * 5 + buttonWidth * 4, Bounds.Y + ButtonMargin, buttonWidth, buttonHeight);
            expandButton.Bounds = new Rectangle(Bounds.X + ButtonMargin * 6 + buttonWidth * 5, Bounds.Y + ButtonMargin, buttonWidth, buttonHeight);

            // Initilialisiert die Schriftfarben des Buttons
            startButton.Foreground = Foreground;
            autoStartButton.Foreground = Foreground;
            playersButton.Foreground = Foreground;
            expandButton.Foreground = Foreground;
            chatButton.Foreground = Foreground;
            accelerationButton.Foreground = Foreground;

            // Initilisiert die Clickevents der Buttons
            startButton.ButtonClicked += StartButton_ButtonClicked;
            autoStartButton.ButtonClicked += AutoStartButton_ButtonClicked;
            playersButton.ButtonClicked += PlayersButton_ButtonClicked; ;
            expandButton.ButtonClicked += ExpandButton_ButtonClicked;
            chatButton.ButtonClicked += ChatButton_ButtonClicked;
            accelerationButton.ButtonClicked += AccelerationButton_ButtonClicked;
        }

        #endregion
        #region Event Methoden

        // Öffnet die Spielerliste
        private void PlayersButton_ButtonClicked()
        {
            currentGame.UILayout.PlayerListContainer.Visible = !currentGame.UILayout.PlayerListContainer.Visible;
        }

        // Vergrößert oder verkleinert das Optionsmeu
        private async void ExpandButton_ButtonClicked()
        {
            if (IsExpanded)
            {
                await ExpandCollapseAsync(false);
            }
            else
            {
                await ExpandCollapseAsync(true);
            }
        }

        // Aktiviert oder Deaktiviert das Autospawning der Ballons
        private void AutoStartButton_ButtonClicked()
        {
            currentGame.NetworkHandler.InvokeEvent("ToggleAutoStart", 0, false);
        }

        // Startet das Spawning der Ballons beim betätigen des Buttons
        private void StartButton_ButtonClicked()
        {
            currentGame.NetworkHandler.InvokeEvent("ContinueRound", 0, false);
        }

        private void ChatButton_ButtonClicked() =>
            currentGame.UILayout.ChatContainer.Visible = !currentGame.UILayout.ChatContainer.Visible;

        private void AccelerationButton_ButtonClicked()
        {
            currentGame.NetworkHandler.InvokeEvent("Accelerate", 0, false);
        }

        #endregion
        #region Async Methoden

        public async Task ExpandCollapseAsync(bool expanding)
        {
            if (expanding)
                await ExpandToAsync(expandButton.Bounds.Width * 6 + ButtonMargin * 7);
            else
                await CollapseToAsync(expandButton.Bounds.Width + ButtonMargin * 3);
        }

        // Vergrößert das Fenster zur angegebenen Breite
        public async Task ExpandToAsync(int aimSize)
        {
            int startPos = Bounds.X;
            int startWidth = Bounds.Width;

            IsExpanded = true;

            while (Bounds.Width <= aimSize)
            {
                Bounds = new Rectangle(Bounds.X - 10, Bounds.Y, Bounds.Width + 20, Bounds.Height);
                expandButton.Bounds = new Rectangle(expandButton.Bounds.X + 10, expandButton.Bounds.Y, expandButton.Bounds.Width, expandButton.Bounds.Height);
                await Task.Delay(5);
            }
            expandButton.Bounds = new Rectangle(Bounds.X + ButtonMargin * 6 + expandButton.Bounds.Width * 5, Bounds.Y + ButtonMargin, expandButton.Bounds.Width, expandButton.Bounds.Height);
        }

        // Verkleinert das Fenster zur angegebenen Breite
        public async Task CollapseToAsync(int aimSize)
        {
            int startPos = Bounds.X;
            int startWidth = Bounds.Width;
            IsExpanded = false;
            while (Bounds.Width >= aimSize)
            {
                Bounds = new Rectangle(Bounds.X + 10, Bounds.Y, Bounds.Width - 20, Bounds.Height);
                expandButton.Bounds = new Rectangle(expandButton.Bounds.X - 10, expandButton.Bounds.Y, expandButton.Bounds.Width, expandButton.Bounds.Height);
                await Task.Delay(5);
            }
            expandButton.Bounds = new Rectangle(Bounds.X + ButtonMargin, Bounds.Y + ButtonMargin, expandButton.Bounds.Width, expandButton.Bounds.Height);
        }

        #endregion
    }
}
