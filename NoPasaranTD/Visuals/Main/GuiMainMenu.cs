using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Logic;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using NoPasaranTD.Networking;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiMainMenu : GuiComponent
    {
        #region Buttons

        private ButtonContainer singleplayerButton;
        private ButtonContainer multiplayerButton;

        private ButtonContainer closeButton;

        private Game backgroundGame;

        private Rectangle transparancyLayer = new Rectangle(0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight);
        private Rectangle randomTextRegion = new Rectangle(50, 60, 200, 200);

        private readonly string randomText = "";

        // Option Werte für die Random Title Animation
        private float scaleFactor = 1f; // Um welchen Faktor soll zurzeit skaliert werden
        private readonly float scaleVelocity = 0.001f; // Die Skaliergeschwindigkeit
        private byte currentDirection = 0; // Soll nach innen (0) oder nach aussen (1) skaliert werden

        // Flying Meme Optionen
        private float memePositionX = 0;
        private float memePositionY = 0;
        private readonly float memeVelocity = 0.1f;
        private float memeRotation = -10;
        private float memeSlope = 0;
        private int memeIndex = 0;

        private readonly Random random = new Random();
        private readonly List<Image> memes = ResourceLoader.LoadMemes();
        #endregion

        /// <summary>
        /// Der Einheitliche Button wird als Button Design für alle Buttons genutzt
        /// </summary>
        /// <param name="content"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public ButtonContainer InitButton(string content, Rectangle bounds)
        {
            return new ButtonContainer()
            {
                Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
                BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
                Foreground = new SolidBrush(Color.Black),
                Margin = 3,
                StringFont = StandartHeader2Font,
                Bounds = bounds,
                Content = content,
            };
        }

        public GuiMainMenu()
        {
            StaticEngine.TickAcceleration = 1;

            List<string> list = ResourceLoader.DichterUndDenker();
            randomText = list[random.Next(list.Count - 1)];

            memeIndex = random.Next(0, memes.Count);
            memePositionY = random.Next(100, StaticEngine.RenderHeight - 100);
            memeSlope = (float)1 / random.Next(-30, 30);
            Decorate();
        }

        public override void Dispose()
        {
            backgroundGame.Dispose();
            foreach (Image meme in memes)
            {
                meme.Dispose();
            }

            memes.Clear();
        }

        private void Decorate()
        {
            // Standartgrößen der Buttons
            int margin = 10;
            int buttonHeight = 50;
            int buttonWidth = 300;

            // Buttons werden initialisiert
            singleplayerButton = InitButton("Singleplayer", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 - buttonHeight - margin, buttonWidth, buttonHeight));
            multiplayerButton = InitButton("Multiplayer", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2, buttonWidth, buttonHeight));

            closeButton = InitButton("X", new Rectangle(StaticEngine.RenderWidth - 45, 5, 40, 40));

            // Events werden Aboniiert
            singleplayerButton.ButtonClicked += SingleplayerButton_ButtonClicked;
            multiplayerButton.ButtonClicked += MultiplayerButton_ButtonClicked;

            closeButton.ButtonClicked += CloseButton_ButtonClicked;

            Map map = MapData.GetMapByFileName("spentagon");
            map.Initialize();

            backgroundGame = new Game(map, new NetworkHandler());
            {
                // UILayout unsichtbar und inaktiv schalten
                backgroundGame.UILayout.Visible = false;
                backgroundGame.GodMode = true;
                backgroundGame.WaveManager.AutoStart = true;

                // Größe der Türme der Hintergrundspielszene
                Size towerCanonSize = StaticInfo.GetTowerSize(typeof(TowerCanon));
                Size towerAtillerySize = StaticInfo.GetTowerSize(typeof(TowerArtillery));

                // Setzt die Tower in das Hintergrundspiel ein
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(520, 260), towerCanonSize), Level = 2, });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(855, 320), towerCanonSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(590, 530), towerCanonSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(160, 160), towerCanonSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(740, 175), towerCanonSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(225, 390), towerCanonSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerArtillery() { Hitbox = new Rectangle(new Point(460, 5), towerAtillerySize), Level = 2 });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerArtillery() { Hitbox = new Rectangle(new Point(800, 630), towerAtillerySize), Level = 2 });
            }
        }

        public override void Render(Graphics g)
        {

            // Der Titel
            string title = "No Pasaran! TD";
            Size titleSize = TextRenderer.MeasureText(title, StandartTitle1Font);

            // Rendert das Hintergrundspiel
            backgroundGame.Render(g);

            Matrix currentTransform = g.Transform;

            g.TranslateTransform(memePositionX, memePositionY);
            g.RotateTransform(memeRotation);

            g.DrawImage(memes[memeIndex], 0, 0, 150, 100);

            g.Transform = currentTransform;

            // Zeichnet das Transparency Layer
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 150, 150, 150)), transparancyLayer);

            // Rendert den Title
            g.DrawString(title, StandartTitle1Font, Brushes.Black, StaticEngine.RenderWidth / 2 - titleSize.Width / 2, StaticEngine.RenderHeight / 4 - titleSize.Height);

            // Rendert alle UI Elemente           
            singleplayerButton.Render(g);
            multiplayerButton.Render(g);

            closeButton.Render(g);


            g.TranslateTransform(randomTextRegion.X + randomTextRegion.Width / 2, randomTextRegion.Y + randomTextRegion.Height / 2);
            g.RotateTransform(-25);
            g.ScaleTransform(scaleFactor, scaleFactor);

            g.DrawString(randomText, StandartHeader2Font, Brushes.Yellow, randomTextRegion);

            g.Transform = currentTransform;
        }

        public override void MouseDown(MouseEventArgs e)
        {
            singleplayerButton.MouseDown(e);
            multiplayerButton.MouseDown(e);

            closeButton.MouseDown(e);
        }

        public override void Update()
        {
            backgroundGame.Update();

            singleplayerButton.Update();
            multiplayerButton.Update();

            closeButton.Update();

            // Skaliert den Random Text nach innen
            if (currentDirection == 0 && StaticEngine.ElapsedTicks % 5 == 0)
            {
                scaleFactor -= scaleVelocity;
                if (scaleFactor <= 0.9)
                {
                    currentDirection = 1;
                }
            }
            else if (currentDirection == 1 && StaticEngine.ElapsedTicks % 5 == 0) // Skaliert den Random Text nach aussen
            {
                scaleFactor += scaleVelocity;
                if (scaleFactor >= 1)
                {
                    currentDirection = 0;
                }
            }

            // Aktualisiert die Memebild Position
            memePositionX += memeVelocity;
            memePositionY += memeSlope;
            memeRotation += 0.01f;

            // Überprüft ob das derzeitige Meme noch valide ist
            if (memePositionX >= StaticEngine.RenderWidth || memePositionY >= StaticEngine.RenderHeight || memePositionY + memes[memeIndex].Height <= 0)
            {
                memeIndex = random.Next(0, memes.Count);
                memePositionY = random.Next(200, StaticEngine.RenderHeight - 200);
                memePositionX = -memes[memeIndex].Width;
                memeSlope = (float)1 / random.Next(-30, 30);
                memeRotation = random.Next(-40, 40);

            }
        }

        // Öffnet die Multiplayerfunktion des Spieles
        private void MultiplayerButton_ButtonClicked()
        {
            Program.LoadScreen(new GuiLobbyMenu());
        }

        // Öffnet die Singleplayerfunktionen des Spieles
        private void SingleplayerButton_ButtonClicked()
        {
            Program.LoadScreen(new GuiSelectMap());
        }

        // Schließt die Applikation
        private void CloseButton_ButtonClicked()
        {
            Program.Shutdown();
        }

        private void CreditsButton_ButtonClicked()
        {
            // TODO: Credits Screen
        }

        private void OptionsButton_ButtonClicked()
        {
            // TODO: Options Screen
        }

        private void TutorialButton_ButtonClicked()
        {
            // TODO: Tutorial Screen
        }
    }
}
