using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Visuals.Ingame.GameOver
{
    public class GuiGameOver:GuiComponent
    {
        private ButtonContainer btnReturnLobby { get; }
        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(175,255,0,0)), 0,0,StaticEngine.RenderWidth,StaticEngine.RenderHeight);  
        }

        public GuiGameOver() 
        {
            btnReturnLobby =  new ButtonContainer
            {
                Bounds = new Rectangle(20,20,StaticEngine.RenderWidth,StaticEngine.RenderHeight),
                Content = string "wdad",
                StringFont = StandartText1Font,
                Foreground = Brushes.Black,
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Blue,
                Margin = 2
            };     
        
        }
    }
}
