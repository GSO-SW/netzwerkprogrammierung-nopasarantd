﻿using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using NoPasaranTD.Engine;

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
        private int buttonMargin = 5;

        // Ist das Control ein- oder ausgeklappt
        private bool isExpanded = true;

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
        private readonly ButtonContainer expandButton = new ButtonContainer()
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
            if (!Visible) return;

            g.FillRectangle(Background, Bounds);

            // Zeigt den Startbutton nur wenn die Runde pausuert ist
            if (currentGame.WaveManager.IsRoundCompleted && isExpanded)
                startButton.Render(g);

            // Zeigt die anderen Buttons nur wenn das Menu expandiert ist
            if (isExpanded)
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
                autoStartButton.Content = "⤻";
            else
                autoStartButton.Content = "⭯";

            if (isExpanded)
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
            if (isExpanded)
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

        public void Init(Game gamer)
        {
            int buttonWidth = (Bounds.Width - 7 * buttonMargin)/6;
            int buttonHeight = Bounds.Height - buttonMargin*2;

            currentGame = gamer;

            // Initialisiert die Grenzen der Buttons
            startButton.Bounds = new Rectangle( Bounds.X + buttonMargin, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            accelerationButton.Bounds = new Rectangle( Bounds.X + buttonMargin * 2 + buttonWidth, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            autoStartButton.Bounds = new Rectangle( Bounds.X + buttonMargin * 3 + buttonWidth * 2, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            playersButton.Bounds = new Rectangle( Bounds.X + buttonMargin * 4 + buttonWidth * 3, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            chatButton.Bounds = new Rectangle( Bounds.X + buttonMargin * 5 + buttonWidth * 4, Bounds.Y + buttonMargin, buttonWidth, buttonHeight);
            expandButton.Bounds = new Rectangle( Bounds.X + buttonMargin * 6 + buttonWidth * 5, Bounds.Y + buttonMargin, buttonWidth , buttonHeight);

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
        private void PlayersButton_ButtonClicked() =>
            currentGame.UILayout.PlayerListContainer.Visible = !currentGame.UILayout.PlayerListContainer.Visible;

        // Vergrößert oder verkleinert das Optionsmeu
        private async void ExpandButton_ButtonClicked()
        {           
            if (!isExpanded)
                await ExpandToAsync(expandButton.Bounds.Width * 6 + buttonMargin * 7);
            else
                await CollapseToAsync(expandButton.Bounds.Width + buttonMargin *2);           
        }

        // Aktiviert oder Deaktiviert das Autospawning der Ballons
        private void AutoStartButton_ButtonClicked()
        {           
            currentGame.NetworkHandler.InvokeEvent("ToggleAutoStart", 0);
        }

        // Startet das Spawning der Ballons beim betätigen des Buttons
        private void StartButton_ButtonClicked() =>
            currentGame.NetworkHandler.InvokeEvent("ContinueRound", 0);

        private void ChatButton_ButtonClicked() =>
            currentGame.UILayout.ChatContainer.Visible = !currentGame.UILayout.ChatContainer.Visible;

        private void AccelerationButton_ButtonClicked() =>
            currentGame.NetworkHandler.InvokeEvent("Accelerate", 0);

        #endregion
        #region Async Methoden

        // Vergrößert das Fenster zur angegebenen Breite
        private async Task ExpandToAsync(int aimSize)
        {
            int startPos = Bounds.X;
            int startWidth = Bounds.Width;
            isExpanded = !isExpanded;
            while (Bounds.Width <= aimSize)
            {
                Bounds= new Rectangle(Bounds.X -10,Bounds.Y,Bounds.Width+20,Bounds.Height);
                expandButton.Bounds = new Rectangle(expandButton.Bounds.X + 10, expandButton.Bounds.Y, expandButton.Bounds.Width, expandButton.Bounds.Height);
                await Task.Delay(5);
            }
            Bounds = new Rectangle(startPos + startWidth/2 - aimSize/2, Bounds.Y, aimSize, Bounds.Height);
            expandButton.Bounds = new Rectangle(Bounds.X + buttonMargin * 6 + expandButton.Bounds.Width * 5, Bounds.Y + buttonMargin, expandButton.Bounds.Width, expandButton.Bounds.Height);
        }

        // Verkleinert das Fenster zur angegebenen Breite
        private async Task CollapseToAsync(int aimSize)
        {
            int startPos = Bounds.X;
            int startWidth = Bounds.Width;
            isExpanded = !isExpanded;
            while (Bounds.Width >= aimSize)
            {
                Bounds = new Rectangle(Bounds.X + 10, Bounds.Y, Bounds.Width - 20, Bounds.Height);
                expandButton.Bounds = new Rectangle(expandButton.Bounds.X - 10, expandButton.Bounds.Y, expandButton.Bounds.Width, expandButton.Bounds.Height);
                await Task.Delay(5);
            }
            Bounds = new Rectangle(startPos + startWidth / 2 - aimSize/2, Bounds.Y, aimSize, Bounds.Height);
            expandButton.Bounds = new Rectangle(Bounds.X + buttonMargin, Bounds.Y + buttonMargin, expandButton.Bounds.Width, expandButton.Bounds.Height);
        }

        #endregion
    }
}
