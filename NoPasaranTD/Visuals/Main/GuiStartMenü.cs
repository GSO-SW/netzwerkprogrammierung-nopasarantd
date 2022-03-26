using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Model;
using NoPasaranTD.Model.Towers;
using NoPasaranTD.Networking;
using NoPasaranTD.Utilities;
using NoPasaranTD.Visuals.Ingame;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiStartMenü : GuiComponent, IDisposable
    {
        #region Buttons

        private ButtonContainer singleplayerButton;
        private ButtonContainer multiplayerButton;
        private ButtonContainer tutorialButton;

        private ButtonContainer optionsButton;
        private ButtonContainer creditsButton;

        private ButtonContainer closeButton;

        private Game backgroundGame;

        private Rectangle transparancyLayer = new Rectangle(0, 0, StaticEngine.RenderWidth, StaticEngine.RenderHeight);
        private Rectangle randomTextRegion = new Rectangle(50, 60, 200, 200);

        private string randomText = "";

        // Option Werte für die Random Title Animation
        private float scaleFactor = 1f; // Um welchen Faktor soll zurzeit skaliert werden
        private float scaleVelocity = 0.001f; // Die Skaliergeschwindigkeit
        private byte currentDirection = 0; // Soll nach innen (0) oder nach aussen (1) skaliert werden

        private int memeCounter = 1;
        private float memePositionX= 0;
        private float memePositionY= 0;
        private float memeVelocity = 0.1f;
        private float memeRotation = -10;
        private float memeSlope = 0;

        private Random random;
        private List<Image> memes = ResourceLoader.LoadMemes();
        private StaticDisplay currentDisplay;
        #endregion

        /// <summary>
        /// Der Einheitliche Button
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

        public GuiStartMenü()
        {
            random = new Random();
            List<string> list = ResourceLoader.DichterUndDenker();
            randomText = list[random.Next(list.Count - 1)];

            memePositionY = random.Next(100, StaticEngine.RenderHeight - 100);
            memeSlope = (float)1 / random.Next(-30, 30);
        }

        public void Init(StaticDisplay staticDisplay)
        {
            // Standartgrößen der Buttons
            int margin = 10;
            int buttonHeight = 50;
            int buttonWidth = 300;

            currentDisplay = staticDisplay;

            // Buttons werden initialisiert
            singleplayerButton = InitButton("Singleplayer",new Rectangle(StaticEngine.RenderWidth/2 - buttonWidth/2,StaticEngine.RenderHeight / 2 - buttonHeight - margin,buttonWidth,buttonHeight));
            multiplayerButton = InitButton("Multiplayer", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2, buttonWidth, buttonHeight));
            tutorialButton = InitButton("Tutorial", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 + buttonHeight + margin, buttonWidth, buttonHeight));

            optionsButton = InitButton("Options", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 + buttonHeight*2 + margin*2, buttonWidth/2 -margin/2, buttonHeight));
            creditsButton = InitButton("Credits", new Rectangle(StaticEngine.RenderWidth / 2 + margin, StaticEngine.RenderHeight / 2 + buttonHeight *2 + margin*2, buttonWidth / 2 - margin, buttonHeight));

            closeButton = InitButton("X", new Rectangle(StaticEngine.RenderWidth - 45, 5, 40, 40));

            // Events werden Aboniiert
            singleplayerButton.ButtonClicked += SingleplayerButton_ButtonClicked;
            multiplayerButton.ButtonClicked += MultiplayerButton_ButtonClicked;
            tutorialButton.ButtonClicked += TutorialButton_ButtonClicked;

            optionsButton.ButtonClicked += OptionsButton_ButtonClicked;
            creditsButton.ButtonClicked += CreditsButton_ButtonClicked;

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
                Size towerSize = StaticInfo.GetTowerSize(typeof(TowerCanon));
                Size towerAtillerySize = StaticInfo.GetTowerSize(typeof(TowerArtillery));

                // Setzt die Tower in das Hintergrundspiel ein
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(520, 260), towerSize), Level = 2, });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(855, 320), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(590, 530), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(160, 160), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(740, 175), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerCanon() { Hitbox = new Rectangle(new Point(225, 390), towerSize) });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerArtillery() { Hitbox = new Rectangle(new Point(460, 30), towerSize), Level = 2 });
                backgroundGame.NetworkHandler.InvokeEvent("AddTower", new TowerArtillery() { Hitbox = new Rectangle(new Point(800, 630), towerSize), Level = 2 });
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

            g.DrawImage(memes[memeCounter], 0, 0,150,100);

            g.Transform = currentTransform;

            // Zeichnet das Transparency Layer
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, 150, 150, 150)), transparancyLayer);

            // Rendert den Title
            g.DrawString(title, StandartTitle1Font, Brushes.Black, StaticEngine.RenderWidth / 2 - titleSize.Width/2, StaticEngine.RenderHeight / 4 -titleSize.Height);

            // Rendert alle UI Elemente           
            singleplayerButton.Render(g);
            multiplayerButton.Render(g);
            tutorialButton.Render(g);

            optionsButton.Render(g);
            creditsButton.Render(g);

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
            tutorialButton.MouseDown(e);

            optionsButton.MouseDown(e);
            creditsButton.MouseDown(e);

            closeButton.MouseDown(e);
        }

        public override void Update()
        {
            backgroundGame.Update();

            singleplayerButton.Update();
            multiplayerButton.Update();
            tutorialButton.Update();

            optionsButton.Update();
            creditsButton.Update();

            closeButton.Update();

            // Skaliert den Random Text nach innen
            if (currentDirection == 0 && StaticEngine.ElapsedTicks % 5 == 0)
            {
                scaleFactor -= scaleVelocity;
                if (scaleFactor <= 0.9)
                    currentDirection = 1;
            }
            else if (currentDirection == 1 && StaticEngine.ElapsedTicks % 5 == 0) // Skaliert den Random Text nach aussen
            {
                scaleFactor += scaleVelocity;
                if (scaleFactor >= 1)
                    currentDirection = 0;
            }
            memePositionX += memeVelocity;
            memePositionY += memeSlope;
            memeRotation += 0.01f;

            if (memePositionX >= StaticEngine.RenderWidth || memePositionY >= StaticEngine.RenderHeight || memePositionY + memes[memeCounter].Height <= 0)
            {
                memeCounter++;
                if (memeCounter == 8)
                    memeCounter = 1;

                memePositionY = random.Next(200,StaticEngine.RenderHeight-200);
                memePositionX = -memes[memeCounter].Width;
                memeSlope = (float)1 / random.Next(-30, 30);
                memeRotation = random.Next(-40, 40);
                
            }
        }

        // Öffnet die Multiplayerfunktion des Spieles
        private void MultiplayerButton_ButtonClicked() =>
            currentDisplay.LoadScreen(new GuiMainMenu());

        // Öffnet die Singleplayerfunktionen des Spieles
        private void SingleplayerButton_ButtonClicked() =>
            Program.LoadScreen(new GuiSelectMap());

        // Schließt die Applikation
        private void CloseButton_ButtonClicked() =>
            currentDisplay.Close();

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

        public override void Dispose() =>
            backgroundGame.Dispose();
    }
}
