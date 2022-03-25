using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int margin = 5;
            int buttonHeight = 30;
            int buttonWidth = 100;

            singleplayerButton = InitButton("Singleplayer",new Rectangle(StaticEngine.RenderWidth/2 - buttonWidth/2,StaticEngine.RenderHeight / 2 - buttonHeight - margin,buttonWidth,buttonHeight));
            multiplayerButton = InitButton("Multiplayer", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 - buttonHeight/2, buttonWidth, buttonHeight));
            tutorialButton = InitButton("Tutorial", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 + buttonHeight + margin, buttonWidth, buttonHeight));

            optionsButton = InitButton("Option", new Rectangle(StaticEngine.RenderWidth / 2 - buttonWidth / 2, StaticEngine.RenderHeight / 2 - buttonHeight - margin, buttonWidth/2 -margin/2, buttonHeight));
        }
    }
}
