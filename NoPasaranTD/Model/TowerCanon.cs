using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using NoPasaranTD.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NoPasaranTD.Model.Towers
{
    public class TowerCanon : Tower
    {
        double shotAnimationLength = 0.2; // in percent of delay   E[0;1]

        SolidBrush bruhBlack, bruhRed, bruhPurple, bruhLightGray, bruhFireColor, bruhDarkGray, brushSlateGray;
        Pen penBlack, penRed, penPurple;
        Stopwatch sw;
        Font font;
        bool justShotSomeUglyAss;
        long time;
        long timeLastShot;
        Utilities.Vector2D lastBalloonPos;
        (int segment, int index) lastBaloonIndex; // Tuple zum speichern des letzten angezielten Ballon Indexes
        int centerX, centerY, sizeX, sizeY;
        uint delay;
        uint strength;
        double range;

        // Das Geschütz der Kanone
        RectangleF barrel;

        private float currentAngle = 0; // Derzeitiger Rotationswinkel des Geschützes der Kanone
        private float aimAngle = 0; // Nächster Rotationswinkel des Geschützes der Kanone
        private ulong ticks = 0; // Anzahl vergangener Ticks

        private float hitboxCornerMargin = 10; // Größe der Polygoneckenabstände zur tatäschlichen Hitbox Ecke

        public TowerCanon()
        {
            GetBalloonFunc = FarthestBallonCheck;
            sizeX = StaticInfo.GetTowerSize(GetType()).Width; sizeY = StaticInfo.GetTowerSize(GetType()).Height;
            justShotSomeUglyAss = false;
            bruhBlack = new SolidBrush(Color.Black); bruhRed = new SolidBrush(Color.Red); bruhPurple = new SolidBrush(Color.Purple); bruhLightGray = new SolidBrush(Color.LightGray);
            bruhFireColor = new SolidBrush(Color.Orange);
            bruhDarkGray = new SolidBrush(Color.DarkGray);
            brushSlateGray = new SolidBrush(Color.SlateGray);
            penBlack = new Pen(Color.Black); penRed = new Pen(Color.Red); penPurple = new Pen(Color.Purple, 2.3f);
            font = new Font(FontFamily.GenericSerif, 7);
            time = 0;
            timeLastShot = 0;
            lastBaloonIndex = (-1, -1);

            delay = Delay;
            strength = Strength;
            range = Range;
            sw = new Stopwatch();
            sw.Start();
        }

        public override void Render(Graphics g)
        {
            centerX = Hitbox.X + Hitbox.Width / 2; centerY = Hitbox.Y + Hitbox.Height / 2; sizeX = Hitbox.Width; sizeY = Hitbox.Height;

            // Koordinaten des Hitboxpolygons
            PointF[] hitboxPolygonCorners = new PointF[8]
            {
                new PointF(Hitbox.X , Hitbox.Y + hitboxCornerMargin),
                new PointF(Hitbox.X + hitboxCornerMargin, Hitbox.Y),

                new PointF(Hitbox.X + Hitbox.Width - hitboxCornerMargin, Hitbox.Y ),
                new PointF(Hitbox.X + Hitbox.Width, Hitbox.Y + hitboxCornerMargin),

                new PointF(Hitbox.X + Hitbox.Width ,Hitbox.Y+Hitbox.Height- hitboxCornerMargin),
                new PointF(Hitbox.X + Hitbox.Width - hitboxCornerMargin ,Hitbox.Y+Hitbox.Height),

                new PointF(Hitbox.X + hitboxCornerMargin,Hitbox.Y+Hitbox.Height),
                new PointF(Hitbox.X , Hitbox.Y + Hitbox.Height - hitboxCornerMargin),

            };

            // Zeichnet die Hitbox des Towers
            g.FillPolygon(brushSlateGray, hitboxPolygonCorners);
            if (IsSelected)
                g.DrawEllipse(penPurple, (float)(centerX - range), (float)(centerY - range), (float)range * 2, (float)range * 2);

            // draws the time left to the next shot in the corner of the tower | generally a debugging/visualization thingy
            //g.DrawString((delay - time + timeLastShot).ToString(), font, bruhLightGray, Hitbox.Location); 

            // Startet eine Rotoations Berechnung für ein neues Target
            RotateBarrel(lastBalloonPos);

            // Das Barrel als Rechteck
            barrel = new RectangleF(-5, 0, 10, 50);
            Matrix currentTransform = g.Transform; // Derzeitige Transformationsmatrix

            // Setzt den Koordinatenursprung auf das Zentrum des Towers
            g.TranslateTransform(centerX, centerY);

            // Erstellt eine Rotationsmatrix mit dem derzeitigen Rotationswinkel
            g.RotateTransform(currentAngle);

            // Das Barrel wird gezeichnet
            g.FillRectangle(bruhDarkGray, barrel);

            // Das innere Viereck wird gezeichnet
            g.FillRectangle(bruhLightGray, -(sizeX * 0.4f) / 2, -(sizeY * 0.4f) / 2, sizeX * 0.4f, sizeY * 0.4f);

            if (justShotSomeUglyAss)
            {
                float factor = System.Math.Max((time - timeLastShot) / (delay * (float)shotAnimationLength), 0);
                if (Math.Pow(
                    (centerX - lastBalloonPos.X) * (centerX - lastBalloonPos.X)
                    + (centerY - lastBalloonPos.Y) * (centerY - lastBalloonPos.Y),
                    0.5) < range)
                    //g.DrawLine(penRed, barrel.X + barrel.Width/2,barrel.Y+barrel.Height,lastBalloonPos.X - centerX ,lastBalloonPos.Y - centerY);
                    g.FillEllipse(bruhFireColor, -barrel.Width + barrel.Width / 4, barrel.Height - 5, 15 * ticks % 30, 15 * ticks % 30); // Feueranimation als Ellipse

                if (timeLastShot + delay * shotAnimationLength < time) justShotSomeUglyAss = false;
            }

            // Die Originalmatrix wird wieder angewandt
            g.Transform = currentTransform;
        }

        public override void Update(Game game)
        {
            ticks++;
            time = sw.ElapsedMilliseconds;

            if (time > timeLastShot + delay)
            {
                (int segment, int index) targetIndex = game.FindTargetForTower(this);
                if (targetIndex.Item1 != -1)
                {
                    timeLastShot = time;
                    lastBaloonIndex = targetIndex;
                    justShotSomeUglyAss = true;
                    lastBalloonPos = game.CurrentMap.GetPathPosition(game.Balloons[targetIndex.Item1][targetIndex.Item2].PathPosition);
                    game.DamageBalloon(targetIndex.Item1, targetIndex.Item2, (int)strength, this); // TODO: uint to int could be an oof conversion
                }
            }
            if (lastBaloonIndex.Item1 != -1 && game.Balloons[lastBaloonIndex.Item1].Count > lastBaloonIndex.Item2) lastBalloonPos = game.CurrentMap.GetPathPosition(game.Balloons[lastBaloonIndex.Item1][lastBaloonIndex.Item2].PathPosition);

            if (currentAngle < aimAngle && ticks % 9 == 0 && aimAngle - currentAngle > 5)
                currentAngle += 4.5F;
            else if (currentAngle > aimAngle && ticks % 9 == 0 && currentAngle - aimAngle > 5)
                currentAngle -= 4.5F;
        }

        // Weißt dem Tower einen neuen Ziel-Rotationswinkel zu
        private void RotateBarrel(Vector2D targetPosition)
        {
            Point center = new Point(Hitbox.X + Hitbox.Width / 2, Hitbox.Y + Hitbox.Height / 2);
            Vector2D vecCenterTarget = new Vector2D(targetPosition.X - center.X, (targetPosition.Y - center.Y) * -1); // Verbindungsvektor zwischen dem Zentrum und dem vordersten Balloon
            aimAngle = (90 + (float)(vecCenterTarget.Angle / Math.PI) * 180) * -1; // Berechnen des Winkels des Vekors zu den Achsen und Umrechung von RAD zu DEG
        }

        public override string ToString() => "Canon";
    }
}
