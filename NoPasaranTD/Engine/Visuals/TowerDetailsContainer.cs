﻿using NoPasaranTD.Model;
using System.Drawing;
using System.Windows.Forms;

namespace NoPasaranTD.Engine.Visuals
{
    /// <summary>
    /// Container für die Details und Optionen eines Towers
    /// </summary>
    public class TowerDetailsContainer : GuiComponent
    {
        // Der Button für das schließen des Fensters
        private ButtonContainer closeButton = new ButtonContainer();
        // Der Button für das starten eines Towers Upgradevorgang
        private ButtonContainer upgradeButton = new ButtonContainer();
        // Der Button für das starten eines Towers Verkaufsvorgang 
        private ButtonContainer sellButton = new ButtonContainer();

        /// <summary>
        /// Der ausgewählte Tower
        /// </summary>
        public Tower Context { get; set; }

        /// <summary>
        /// Die Hintergrundfarbe des Containers
        /// </summary>
        public Brush Background { get; set; }

        /// <summary>
        /// Die Schriftfarbe der Texte
        /// </summary>
        public Brush Foreground { get; set; }

        /// <summary>
        /// Die Fonts der Buttonelemente
        /// </summary>
        public Font ButtonFont { get; set; }

        /// <summary>
        /// Die Fonts der Texte
        /// </summary>
        public Font TextFont { get; set; }

        private Game currentGame;

        public TowerDetailsContainer() { }

        public override void Render(Graphics g)
        {
            if (Visible)
            {
                // Zeichnet den Hintergrund des Fensters
                g.FillRectangle(Background, Bounds);

                closeButton.Render(g);
                upgradeButton.Render(g);
                sellButton.Render(g);

                // Die normale Texthöhe der Beschreibung
                int normalTextHeight = TextRenderer.MeasureText(Context.NumberKills.ToString(), TextFont).Height;

                // Anzeige des Balloon Name
                g.DrawString(Context.ToString(), GuiComponent.StandartHeader2Font, Foreground, closeButton.Bounds.X + closeButton.Bounds.Width + 5, Bounds.Y + 5);
                // Anzeige der getroffenen Balloons
                g.DrawString("Layers destroyed: " + Context.NumberKills, TextFont, Foreground, Bounds.X + 5, Bounds.Y + 40);
                // Anzeige der Range 
                g.DrawString("Range:  " + Context.Range, TextFont, Foreground, Bounds.X + 5, Bounds.Y + 40 + normalTextHeight + 5);
                // Anzeige der Stärke
                g.DrawString("Strength:  " + Context.Strength, TextFont, Foreground, Bounds.X + 5, Bounds.Y + 40 + (normalTextHeight + 5) * 2);
                // Anzeige der Geschwindigkeit
                g.DrawString("Cooldown Time:  " + Context.Delay, TextFont, Foreground, Bounds.X + 5, Bounds.Y + 40 + (normalTextHeight + 5) * 3);
                // TODO: Anzeige des Verkaufspreises
                // TODO: Anzeige des TowerLevels
            }
        }

        /// <summary>
        /// Initialisiert alle visuellen Componenten des Containers ! Muss vor der Nutzung aufgerufen werden !
        /// </summary>
        /// <param name="game">Das derzeitige Spiel</param>
        public void Init(Game game)
        {
            currentGame = game;

            // Init CloseButton
            closeButton = new ButtonContainer()
            {
                Bounds = new Rectangle(Bounds.X + 5, Bounds.Y + 5, 20, 20),
                Content = "X",
                Background = Brushes.Gray,
                BorderBrush = new SolidBrush(Color.FromArgb(32, 125, 199)),
                Margin = 1,
                StringFont = ButtonFont,
                Foreground = Brushes.Black
            };
            closeButton.ButtonClicked += CloseButton_ButtonClicked;

            // Init UpgradeButton
            upgradeButton = new ButtonContainer()
            {
                Bounds = new Rectangle(Bounds.X + 5, Bounds.Y + Bounds.Height - 35, Bounds.Width / 2 - 10,30),
                Content = "Upgrade",
                Background = Brushes.Gray,
                BorderBrush = new SolidBrush(Color.FromArgb(32, 125, 199)),
                Margin = 1,
                StringFont = ButtonFont,
                Foreground = Brushes.Black
            };
            upgradeButton.ButtonClicked += UpgradeButton_ButtonClicked;

            // Init SellButton
            sellButton = new ButtonContainer()
            {
                Bounds = new Rectangle(Bounds.X + Bounds.Width / 2 + 5, Bounds.Y + Bounds.Height - 35, Bounds.Width / 2 - 10, 30),
                Content = "Sell",
                Background = Brushes.Gray,
                BorderBrush = new SolidBrush(Color.FromArgb(32, 125, 199)),
                Margin = 1,
                StringFont = ButtonFont,
                Foreground = Brushes.Black
            }; sellButton.ButtonClicked += SellButton_ButtonClicked;
        }

        // Wenn der Tower verkauft wird soll das Fenster geschlossen werden.
        private void SellButton_ButtonClicked()
        {
            currentGame.Towers.Remove(Context);
            Visible = false;
        }

        // Logik wenn der Tower geupgraded werden soll
        private void UpgradeButton_ButtonClicked()
        {
            // TODO: Tower Upgraden
        }

        public override void MouseDown(MouseEventArgs e)
        {
            closeButton.MouseDown(e);
            sellButton.MouseDown(e);
            upgradeButton.MouseDown(e);
        }
           
        // Versteckt das Fenster wenn der Schließenbutton betätigt wird
        private void CloseButton_ButtonClicked()
        {
            Visible = false;
            Context.IsSelected = false;
        } 
            
    }
}
