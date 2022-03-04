using NoPasaranTD.Engine;
using System.Drawing;

namespace NoPasaranTD.Model
{
    internal class TowerTest : Tower
    {
        public override void Render(Graphics g) 
        {
            g.FillRectangle(Brushes.Blue, Hitbox);

            g.DrawEllipse(Pens.Black, 
                (float)(Hitbox.X - Range),
                (float)(Hitbox.Y - Range),
                (float)(Hitbox.Width + Range * 2),
                (float)(Hitbox.Height + Range * 2));
        }

        public override void Update(Game game, int targetIndex)
        {
            if(game.CurrentTick % Delay == 0)
            {
                if (targetIndex != -1)
                    game.DamageBalloon(targetIndex, 1);
            }
        }
    }
}
