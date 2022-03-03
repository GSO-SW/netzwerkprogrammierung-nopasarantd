using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using System.Diagnostics;
using System.Drawing;

namespace NoPasaranTD.Model
{
    public abstract class Tower
    {
        public Rectangle Hitbox { get; set; } // TODO should size of rectangle be accessable?
        public uint Level { get; set; }

        public uint Strength { get => StaticInfo.GetTowerDelay(GetType()); }
        public uint Delay { get => StaticInfo.GetTowerDelay(GetType()); }
        public double Range { get => StaticInfo.GetTowerRange(GetType()); }
        
        public abstract void Render(Graphics g);
        public abstract void Update(Game game, int targetIndex);
    }

    public class TowerCanon : Tower
    {
        double shotAnimationLength = 0.2; // in percent of delay   E[0;1]

        SolidBrush bruhBlack, bruhRed, bruhPurple;
        Pen penBlack, penRed, penPurple;
        Stopwatch sw;
        Font font;
        bool justShotSomeUglyAss;
        long time;
        long timeLastShot;
        Utilities.Vector2D lastBalloonPos;
        int centerX, centerY, sizeX, sizeY;
        uint delay;
        uint strength;
        double range;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="posX">centralized</param>
        /// <param name="posY">centralized</param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        public TowerCanon(int posX, int posY)
        {
            sizeX = StaticInfo.GetTowerSize(GetType()).Width; sizeY = StaticInfo.GetTowerSize(GetType()).Height;
            Hitbox = new Rectangle(posX-sizeX/2, posY-sizeY/2, sizeX, sizeY);
            justShotSomeUglyAss = false;
            bruhBlack = new SolidBrush(Color.Black); bruhRed = new SolidBrush(Color.Red); bruhPurple = new SolidBrush(Color.Purple);
            penBlack = new Pen(Color.Black); penRed = new Pen(Color.Red); penPurple = new Pen(Color.Purple, 2.3f);
            font = SystemFonts.DefaultFont;
            time = 0;
            timeLastShot = 0;
            
            delay = Delay;
            strength = Strength;
            range = Range;
            sw = new Stopwatch();
            sw.Start();
        }
        public override void Render(Graphics g)
        {
            centerX = Hitbox.X + Hitbox.Width / 2; centerY = Hitbox.Y + Hitbox.Height / 2; sizeX = Hitbox.Width; sizeY = Hitbox.Height;

            g.FillRectangle(bruhBlack, Hitbox);
            g.DrawEllipse(penPurple, (float)(centerX - range), (float)(centerY - range), (float)range * 2, (float)range * 2);
            g.DrawString((delay - time + timeLastShot).ToString(), font, bruhBlack, Hitbox.Location);
            
            if (justShotSomeUglyAss)
            {
                float factor = 1 - System.Math.Max( (time - timeLastShot) / (delay * (float)shotAnimationLength), 0);
                float halfSizeX = sizeX*0.5f, halfSizeY = sizeY*0.5f;
                g.DrawLine(penRed, centerX, centerY, lastBalloonPos.X, lastBalloonPos.Y);
                g.FillEllipse(bruhPurple, centerX-halfSizeX*factor, centerY-halfSizeY*factor, sizeX*factor, sizeY*factor);
                g.FillRectangle(bruhRed, centerX-sizeX*0.2f, centerY-sizeY*0.2f, sizeX*0.4f, sizeY*0.4f);
                if (timeLastShot + delay * shotAnimationLength < time) justShotSomeUglyAss = false;
            }
        }

        public override void Update(Game game, int targetIndex)
        {
            time = sw.ElapsedMilliseconds;

            if (targetIndex != -1 && time > timeLastShot + delay)
            {
                timeLastShot = time;
                justShotSomeUglyAss = true;
                lastBalloonPos = game.CurrentMap.GetPathPosition(game.Balloons[targetIndex].PathPosition);
                game.DamageBalloon(targetIndex, (int)strength); // TODO: uint to int could be an oof conversion
            }
            
        }
    }
}
