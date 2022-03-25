using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoPasaranTD.Visuals.Main
{
    public class GuiStartMenü : GuiComponent
    {
        #region Buttons

        private ButtonContainer singleplayerButton;
        private ButtonContainer multiplayerButton;
        private ButtonContainer tutorialButton;
        private ButtonContainer optionsButton;
        private ButtonContainer creditsButton;
        private ButtonContainer closeButton;

        #endregion

        public ButtonContainer InitButton(string content, Rectangle bounds)
        {
            return new ButtonContainer()
            {
                Background = new SolidBrush(Color.FromArgb(132, 140, 156)),
                BorderBrush = new SolidBrush(Color.FromArgb(108, 113, 122)),
                Foreground = new SolidBrush(Color.Black),
                Margin = 1,
                StringFont = StandartHeader2Font,
                Bounds = bounds,
                Content = content,
            };
        }

        public GuiStartMenü()
        {

        }

        public void Init()
        {
            // Standartgrößen der Buttons
            int margin = 5;
            int buttonHeight = 30;
            int buttonWidth = 100;

            // Buttons werden initialisiert
            singleplayerButton = InitButton("Singleplayer",new Rectangle(StaticEngine.RenderWidth/2 - buttonWidth/2,StaticEngine.RenderHeight / 2 - buttonHeight - margin,buttonWidth,buttonHeight));
            multiplayerButton = InitButton("Multiplayer", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 - buttonHeight/2, buttonWidth, buttonHeight));
            tutorialButton = InitButton("Tutorial", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 + buttonHeight + margin, buttonWidth, buttonHeight));

            optionsButton = InitButton("Option", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 - buttonHeight*2 - margin*2, buttonWidth/2 -margin/2, buttonHeight));
            creditsButton = InitButton("Credits", new Rectangle(StaticEngine.RenderWidth / 2 + margin, StaticEngine.RenderHeight / 2 - buttonHeight - margin, buttonWidth / 2 - margin / 2, buttonHeight));

            // Events werden Aboniiert
            singleplayerButton.ButtonClicked += SingleplayerButton_ButtonClicked;
            multiplayerButton.ButtonClicked += MultiplayerButton_ButtonClicked;
            tutorialButton.ButtonClicked += TutorialButton_ButtonClicked;
            optionsButton.ButtonClicked += OptionsButton_ButtonClicked;
            creditsButton.ButtonClicked += CreditsButton_ButtonClicked;
        }

        public override void Render(Graphics g)
        {
            singleplayerButton.Render(g);
            multiplayerButton.Render(g);
            tutorialButton.Render(g);
            optionsButton.Render(g);
            creditsButton.Render(g);
        }

        public override void MouseDown(MouseEventArgs e)
        {
            singleplayerButton.MouseDown(e);
            multiplayerButton.MouseDown(e);
            tutorialButton.MouseDown(e);
            optionsButton.MouseDown(e);
            creditsButton.MouseDown(e);
        }

        public override void Update()
        {
            singleplayerButton.Update();
            multiplayerButton.Update();
            tutorialButton.Update();
            optionsButton.Update();
            creditsButton.Update();
        }

        private void CreditsButton_ButtonClicked()
        {
        }

        private void OptionsButton_ButtonClicked()
        {
        }

        private void TutorialButton_ButtonClicked()
        {
        }

        private void MultiplayerButton_ButtonClicked()
        {
        }

        private void SingleplayerButton_ButtonClicked()
        {
        }
    }
}
