using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Model.Towers
{
   

    public class TowerCanon : Tower
    {
        double shotAnimationLength = 0.2; // in percent of delay   E[0;1]

        SolidBrush bruhBlack, bruhRed, bruhPurple, bruhLightGray;
        Pen penBlack, penRed, penPurple;
        Stopwatch sw;
        Font font;
        bool justShotSomeUglyAss;
        long time;
        long timeLastShot;
        Utilities.Vector2D lastBalloonPos;
        int lastBaloonIndex;
        int centerX, centerY, sizeX, sizeY;
        uint delay;
        uint strength;
        double range;

        // Das Geschütz der Kanone
        Rectangle barrel;
        Point shootPosition;

        private float currentAngle = 0; // Derzeitiger Rotationswinkel des Geschützes der Kanone
        private float aimAngle = 0; // Nächster Rotationswinkel des Geschützes der Kanone
        private ulong ticks = 0; // Anzahl vergangener Ticks

        
        //private Matrix rotatedTransform = new Matrix(); // Rotierte Transformationsmatrix

        /// <param name="posX">centralized</param>
        /// <param name="posY">centralized</param>
        public TowerCanon()
        {
            sizeX = StaticInfo.GetTowerSize(GetType()).Width; sizeY = StaticInfo.GetTowerSize(GetType()).Height;
            justShotSomeUglyAss = false;
            bruhBlack = new SolidBrush(Color.Black); bruhRed = new SolidBrush(Color.Red); bruhPurple = new SolidBrush(Color.Purple); bruhLightGray = new SolidBrush(Color.LightGray);
            penBlack = new Pen(Color.Black); penRed = new Pen(Color.Red); penPurple = new Pen(Color.Purple, 2.3f);
            font = new Font(FontFamily.GenericSerif, 7);
            time = 0;
            timeLastShot = 0;
            lastBaloonIndex = -1;

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
            // draws the time left to the next shot in the corner of the tower | generally a debugging/visualization thingy
            //g.DrawString((delay - time + timeLastShot).ToString(), font, bruhLightGray, Hitbox.Location); 

           
            RotateBarrel(lastBalloonPos);
            
            barrel = new Rectangle(- 5, - 5, 10, 50);
            Matrix currentTransform = g.Transform; // Derzeitige Transformationsmatrix

            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(currentAngle); // Rotationsmatrix wird auf den derzeitigen Winkel gesetzt
            // Die genutzte Transformationsmatrix ist die Rotationsmatix
            g.FillRectangle(bruhRed, barrel);

            // Die Originalmatrix wird wieder angewandt
            g.Transform = currentTransform;
            if (justShotSomeUglyAss)
            {
                float factor = 1 - System.Math.Max((time - timeLastShot) / (delay * (float)shotAnimationLength), 0);
                float halfSizeX = sizeX * 0.5f, halfSizeY = sizeY * 0.5f;
                if ( Math.Pow(
                    (centerX-lastBalloonPos.X) * (centerX - lastBalloonPos.X)
                    + (centerY-lastBalloonPos.Y) * (centerY - lastBalloonPos.Y),
                    0.5 ) < range )
                    g.DrawLine(penRed, centerX, centerY, lastBalloonPos.X, lastBalloonPos.Y);
                g.FillEllipse(bruhPurple, centerX - halfSizeX * factor, centerY - halfSizeY * factor, sizeX * factor, sizeY * factor);
                g.FillRectangle(bruhRed, centerX - sizeX * 0.2f, centerY - sizeY * 0.2f, sizeX * 0.4f, sizeY * 0.4f);
                if (timeLastShot + delay * shotAnimationLength < time) justShotSomeUglyAss = false;
            }
        }

        public override void Update(Game game, int targetIndex)
        {
            ticks++;
            time = sw.ElapsedMilliseconds;

            if (targetIndex != -1)
            {
                lastBalloonPos = game.CurrentMap.GetPathPosition(game.Balloons[targetIndex].PathPosition);
                if (time > timeLastShot + delay)
                {
                    timeLastShot = time;
                    lastBaloonIndex = targetIndex;
                    justShotSomeUglyAss = true;                    
                    game.DamageBalloon(targetIndex, (int)strength, game.Towers.IndexOf(this)); // TODO: uint to int could be an oof conversion
                }               
            }
            if (lastBaloonIndex != -1 && game.Balloons.Count > lastBaloonIndex) lastBalloonPos = game.CurrentMap.GetPathPosition(game.Balloons[lastBaloonIndex].PathPosition);

            if (currentAngle < aimAngle && ticks % 10 == 1 && aimAngle - currentAngle > 5)
                currentAngle += 2.5F;
            else if (currentAngle > aimAngle && ticks % 10 == 1 && currentAngle - aimAngle > 5)
                currentAngle -= 2.5F;
        }

        private void RotateBarrel(Vector2D targetPosition)
        {
            Point center = new Point(Hitbox.X + Hitbox.Width / 2, Hitbox.Y + Hitbox.Height / 2);
            Vector2D vecCenterTarget = new Vector2D(targetPosition.X - center.X, (targetPosition.Y - center.Y) * -1); // Verbindungsvektor zwischen dem Zentrum und dem vordersten Balloon
            aimAngle = (90 + (float)(vecCenterTarget.Angle / Math.PI) * 180) * -1; // Berechnen des Winkels des Vekors zu den Achsen und umrechung von RAD zu DEG
        }

        public override string ToString() => "Canon";
    }
}
