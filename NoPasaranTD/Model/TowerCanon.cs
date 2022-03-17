using NoPasaranTD.Engine;
using NoPasaranTD.Utilities;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace NoPasaranTD.Model.Towers
{
    [Serializable]
    public class TowerCanon : Tower
    {
        private const float HITBOX_CORNER_MARGIN = 10; // Größe der Polygoneckenabstände zur tatäschlichen Hitbox Ecke
        private const double SHOT_ANIMATION_LENGTH = 0.2d; // in percent of delay   E[0;1]
        private static readonly Pen RANGE_CIRCLE_PEN = new Pen(Color.Purple, 2.3f);

        private (int segment, int index) lastBalloonIndex; // Tuple zum speichern des letzten angezielten Ballon Indexes
        private long time, timeLastShot;
        private bool justShotSomeUglyAss;
        private Vector2D lastBalloonPos;

        private float currentAngle = 0; // Derzeitiger Rotationswinkel des Geschützes der Kanone
        private float aimAngle = 0; // Nächster Rotationswinkel des Geschützes der Kanone

        public TowerCanon()
        {
            lastBalloonIndex = (-1, -1);
            time = 0L; timeLastShot = 0L;
            justShotSomeUglyAss = false;
            lastBalloonPos = new Vector2D(0, 0);
        }

        public override void Render(Graphics g)
        {
            int centerX = Hitbox.X + Hitbox.Width / 2;
            int centerY = Hitbox.Y + Hitbox.Height / 2;

            // Koordinaten des Hitboxpolygons
            PointF[] hitboxPolygonCorners = new PointF[8]
            {
                new PointF(Hitbox.X , Hitbox.Y + HITBOX_CORNER_MARGIN),
                new PointF(Hitbox.X + HITBOX_CORNER_MARGIN, Hitbox.Y),

                new PointF(Hitbox.X + Hitbox.Width - HITBOX_CORNER_MARGIN, Hitbox.Y),
                new PointF(Hitbox.X + Hitbox.Width, Hitbox.Y + HITBOX_CORNER_MARGIN),

                new PointF(Hitbox.X + Hitbox.Width ,Hitbox.Y+Hitbox.Height- HITBOX_CORNER_MARGIN),
                new PointF(Hitbox.X + Hitbox.Width - HITBOX_CORNER_MARGIN ,Hitbox.Y+Hitbox.Height),

                new PointF(Hitbox.X + HITBOX_CORNER_MARGIN,Hitbox.Y+Hitbox.Height),
                new PointF(Hitbox.X , Hitbox.Y + Hitbox.Height - HITBOX_CORNER_MARGIN),
            };

            // Zeichnet die Hitbox des Towers
            if (IsPositionValid || IsPlaced) // Der Tower wird normal gezeichnet wenn dieser gesetzt ist oder seine Position valide ist
                g.FillPolygon(Brushes.SlateGray, hitboxPolygonCorners);
            else if (!IsPlaced) // Ist der Tower nicht gesetzt und die Position ist nicht Valide dann soll dieser einen roten Ground haben
                g.FillPolygon(Brushes.Red, hitboxPolygonCorners);

            if (IsSelected)
                g.DrawEllipse(RANGE_CIRCLE_PEN, (float)(centerX - Range), (float)(centerY - Range), (float)Range * 2, (float)Range * 2);

            // draws the time left to the next shot in the corner of the tower | generally a debugging/visualization thingy
            //g.DrawString((delay - time + timeLastShot).ToString(), font, bruhLightGray, Hitbox.Location); 

            // Startet eine Rotoations Berechnung für ein neues Target
            RotateBarrel(lastBalloonPos);

            // Das Barrel als Rechteck
            RectangleF barrel = new RectangleF(-5, 0, 10, 50);
            Matrix currentTransform = g.Transform; // Derzeitige Transformationsmatrix

            // Setzt den Koordinatenursprung auf das Zentrum des Towers
            g.TranslateTransform(centerX, centerY);

            // Erstellt eine Rotationsmatrix mit dem derzeitigen Rotationswinkel
            g.RotateTransform(currentAngle);

            // Das Barrel wird gezeichnet
            g.FillRectangle(Brushes.DarkGray, barrel);

            // Das innere Viereck wird gezeichnet
            g.FillRectangle(Brushes.LightGray, -(Hitbox.Width * 0.4f) / 2, -(Hitbox.Height * 0.4f) / 2, Hitbox.Width * 0.4f, Hitbox.Height * 0.4f);

            if (justShotSomeUglyAss)
            {
                if (Math.Pow(
                    (centerX - lastBalloonPos.X) * (centerX - lastBalloonPos.X)
                    + (centerY - lastBalloonPos.Y) * (centerY - lastBalloonPos.Y),
                    0.5) < Range)
                    //g.DrawLine(penRed, barrel.X + barrel.Width/2,barrel.Y+barrel.Height,lastBalloonPos.X - centerX ,lastBalloonPos.Y - centerY);
                    g.FillEllipse(Brushes.Orange, -barrel.Width + barrel.Width / 4, barrel.Height - 5, 15 * time % 30, 15 * time % 30); // Feueranimation als Ellipse

                if (timeLastShot + Delay * SHOT_ANIMATION_LENGTH < time) justShotSomeUglyAss = false;
            }

            // Die Originalmatrix wird wieder angewandt
            g.Transform = currentTransform;
        }

        public override void Update(Game game)
        {
            time = game.CurrentTick;
            if (time > timeLastShot + Delay)
            {
                (int segment, int index) targetIndex = game.FindTargetForTower(this);
                if (targetIndex.Item1 != -1)
                {
                    timeLastShot = time;
                    lastBalloonIndex = targetIndex;
                    justShotSomeUglyAss = true;
                    lastBalloonPos = game.CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, game.Balloons[targetIndex.segment][targetIndex.index].PathPosition);
                    game.DamageBalloon(targetIndex.segment, targetIndex.index, (int)Strength, this); // TODO: uint to int could be an oof conversion
                }
            }
            if (lastBalloonIndex.segment != -1 && game.Balloons[lastBalloonIndex.segment].Count > lastBalloonIndex.index) lastBalloonPos = game.CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, game.Balloons[lastBalloonIndex.segment][lastBalloonIndex.index].PathPosition);

            if (currentAngle < aimAngle && time % 9 == 0 && aimAngle - currentAngle > 5)
                currentAngle += 4.5F;
            else if (currentAngle > aimAngle && time % 9 == 0 && currentAngle - aimAngle > 5)
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
