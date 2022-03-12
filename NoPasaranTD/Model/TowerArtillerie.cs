using NoPasaranTD.Data;
using NoPasaranTD.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoPasaranTD.Model
{
    public class TowerArtillerie : Tower
    {
        double shotAnimationLength = 1; // in percent of delay   E[0;1]

        SolidBrush bruhBlack, bruhRed, bruhPurple, bruhLightGray, bruhFireColor, bruhDarkGray, bruhSlateGray, bruhWhite, bruhTransparent, bruhWhiteTransparent;
        Pen penBlack, penRed, penPurple, penWhite, penSlateGray, penOrange;
        Font font;


        Utilities.Vector2D lastBalloonPos;
        Utilities.Vector2D[] lastBalloonPositions;
        Utilities.Vector2D rotationVec = new Utilities.Vector2D(30,30);
        //int lastBalloonIndex = -1;
        //int centerX, centerY, sizeX, sizeY;
        PointF centerCanon = new Point(0,0);
        RectangleF rectPipeTransformed = new RectangleF(), rectCircle1 = new RectangleF(), rectCircle2 = new RectangleF(),
            rectCircle3 = new RectangleF(), rectPipe = new RectangleF();

        //animation related
        int phaseUp = 1; // 1:reloading | 2:firing | 3:waiting
        int phaseDown = 3; // 1:midair | 2:impact | 3:void
        float ratioPipeToHitbox;
        

        uint delay;
        uint strength;
        double range;

        ulong ticks = 0; // Anzahl vergangener Ticks
        ulong tickReloadStart = 0;
        ulong tickFireStart = 0;
        ulong tickLastImpact = 0;
        ulong tickImpactStart = 0;
        ulong tickLength_fireAnimation = 2000;
        ulong tickLength_waitBetween = 1000;

        int maxTargetsLocked = 10;
        int targetCounter = 0;
        
        public TowerArtillerie()
        {
            GetBalloonFunc = FarthestBallonCheck;
            //sizeX = StaticInfo.GetTowerSize(GetType()).Width; sizeY = StaticInfo.GetTowerSize(GetType()).Height;
            bruhBlack = new SolidBrush(Color.Black); bruhRed = new SolidBrush(Color.Red);
            bruhPurple = new SolidBrush(Color.Purple); bruhLightGray = new SolidBrush(Color.LightGray);
            bruhWhite = new SolidBrush(Color.White); bruhFireColor = new SolidBrush(Color.Orange);
            bruhDarkGray = new SolidBrush(Color.DarkGray); bruhSlateGray = new SolidBrush(Color.SlateGray);
            bruhWhiteTransparent = new SolidBrush(Color.FromArgb(200, 255, 255, 255));

            penBlack = new Pen(Color.Black); penRed = new Pen(Color.Red); penPurple = new Pen(Color.Purple, 2.3f);
            penWhite = new Pen(Color.White); penSlateGray = new Pen(Color.SlateGray); penOrange = new Pen(Color.Orange);
            font = new Font(FontFamily.GenericSerif, 7);
            lastBalloonPositions = new Utilities.Vector2D[maxTargetsLocked];

            delay = Delay; // TODO: do math --> ms in ticks, based on current engine tps | currently inaccessable
            delay = 10000;
            strength = Strength;
            range = Range;

            ratioPipeToHitbox = 0.8f;
        }
        public override void Render(Graphics g)
        {
            ulong lTicks = ticks;
            //g.FillRectangle(bruhLightGray, Hitbox);

            // horizontal legs 
            RectangleF leg1 = new RectangleF(
                rectPipe.X - 2, rectPipe.Y - 2,
                rectPipe.Width * 0.7f, rectPipe.Height / 4);
            RectangleF leg2 = new RectangleF(
                rectPipe.X - 2, rectPipe.Y + 2 + rectPipe.Height * 0.75f,
                rectPipe.Width * 0.7f, rectPipe.Height / 4);

            // extra support legs
            float diameter = Math.Min(rectPipe.Width, rectPipe.Height);
            float brickSizeX = rectPipe.Width * 0.2f;
            RectangleF supportLeg1 = new RectangleF(
                rectPipe.X + diameter / 2 - brickSizeX/2, leg1.Y,
                brickSizeX, leg1.Height);
            RectangleF supportLeg2 = new RectangleF(
                rectPipe.X + diameter / 2 - brickSizeX/2, leg2.Y,
                brickSizeX, leg2.Height);
            
            g.FillRectangle(bruhDarkGray, leg1);
            g.FillRectangle(bruhDarkGray, leg2);
            g.DrawRectangle(penSlateGray, leg1.X, leg1.Y, leg1.Width, leg1.Height);
            g.DrawRectangle(penSlateGray, leg2.X, leg2.Y, leg2.Width, leg2.Height);
            g.FillRectangle(bruhFireColor, supportLeg1);
            g.FillRectangle(bruhFireColor, supportLeg2);


            // draws the time left to the next shot in the corner of the tower | generally a debugging/visualization thingy
            //g.DrawString((delay - lTicks + tickReloadStart).ToString(), font, bruhBlack, Hitbox.Location);

            // attack radius
            //g.DrawEllipse(penPurple, Hitbox.X+(int)(Hitbox.Width/2.0-range), Hitbox.Y + (int)(Hitbox.Height/2-range), (int)range*2, (int)range*2);

            
            switch (phaseUp)
            {
                case 1: // animation: reload
                    
                    PointF margin = new PointF(Hitbox.Width*(1-ratioPipeToHitbox)/2, Hitbox.Height*(1-ratioPipeToHitbox)/2);
                    rectPipe = new RectangleF(Hitbox.X+margin.X, Hitbox.Y+margin.Y, Hitbox.Width * ratioPipeToHitbox, Hitbox.Height * ratioPipeToHitbox);
                    float TS1 = TweenService(tickReloadStart, tickReloadStart + delay, 0, diameter/2, lTicks); // für die linke seite
                    float TS2 = TweenServiceExp(tickReloadStart, tickReloadStart + delay, 0, rectPipe.Width - TS1 - diameter / 2, lTicks, 4); // für die rechte seite
                    
                    float upperY = rectPipe.Y,
                        lowerY = rectPipe.Bottom,
                        leftLineX = rectPipe.X + TS1,
                        rightLineX = rectPipe.Right - TS1 - TS2;
                    
                    rectPipeTransformed = new RectangleF(
                        leftLineX ,            upperY,
                        rightLineX-leftLineX,  lowerY-upperY);
                    
                    rectCircle1 = new RectangleF(
                        leftLineX - TS1,       upperY,
                        TS1*2,                 lowerY-upperY);
                    
                    rectCircle2 = new RectangleF(
                        rightLineX - TS1,      upperY,
                        TS1*2,                 lowerY-upperY);

                    rectCircle3 = new RectangleF(
                        rectCircle2.X + rectCircle2.Width * 0.1f, rectCircle2.Y + rectCircle2.Height * 0.1f,
                        rectCircle2.Width * 0.8f, rectCircle2.Height * 0.8f);
                    g.FillEllipse(bruhDarkGray, rectCircle1);
                    g.DrawEllipse(penSlateGray, rectCircle1);
                    g.FillRectangle(bruhDarkGray, rectPipeTransformed);
                    g.DrawLine(penSlateGray, leftLineX, rectPipe.Y, rightLineX, rectPipe.Y);
                    g.DrawLine(penSlateGray, leftLineX, rectPipe.Bottom, rightLineX, rectPipe.Bottom);
                    g.FillEllipse(bruhDarkGray, rectCircle2);
                    g.FillEllipse(bruhBlack, rectCircle3);
                    g.DrawEllipse(penSlateGray, rectCircle2);

                    centerCanon = new PointF(leftLineX,upperY + (lowerY-upperY)/2);

                    if (tickReloadStart + delay < lTicks) { tickFireStart = tickReloadStart + delay + tickLength_waitBetween; phaseUp = 2; }
                    break;
                case 2: // animation: fire!!
                    if (phaseDown != 1 && phaseDown != 2) // [!phaseDown2] could be optional
                    {
                        // base canon
                        g.FillRectangle(bruhDarkGray, rectPipeTransformed);
                        g.FillEllipse(bruhDarkGray, rectCircle1);
                        g.FillEllipse(bruhDarkGray, rectCircle2);
                        g.FillEllipse(bruhBlack, rectCircle3);
                        g.DrawEllipse(penSlateGray, rectCircle2);

                        // blast particles
                        if (ticks % 2 == 0 && lTicks > tickFireStart + tickLength_fireAnimation * 0.1)
                        {
                            rotationVec = rotationVec.Rotated(1);
                            g.DrawLine(penRed, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);
                            rotationVec = rotationVec.Rotated(45);
                            g.DrawLine(penRed, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);
                            rotationVec = rotationVec.Rotated(45);
                            g.DrawLine(penRed, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);
                            rotationVec = rotationVec.Rotated(45);
                            g.DrawLine(penRed, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);

                        }
                        // projectile
                        
                        float TS3 = TweenServiceExp(tickFireStart, tickFireStart + tickLength_fireAnimation,20,100,lTicks,7);
                        float TS4 = TweenServiceExp(tickFireStart+ (ulong)(tickLength_fireAnimation*0.8f), tickFireStart + tickLength_fireAnimation, 255, 0, lTicks, 2);
                        bruhTransparent = new SolidBrush(Color.FromArgb((int)TS4, Color.SlateGray));
                        RectangleF rect = new RectangleF(
                            centerCanon.X - TS3, centerCanon.Y - TS3,
                            TS3*2, TS3*2
                            );
                        g.FillEllipse(bruhTransparent, rect);
                        if (lTicks > tickFireStart + tickLength_fireAnimation)
                        { phaseUp = 3; phaseDown = 1; }
                    }
                    
                    break;
                case 3: // wait
                    g.FillRectangle(bruhDarkGray, rectPipeTransformed);
                    g.FillEllipse(bruhDarkGray, rectCircle1);
                    g.FillEllipse(bruhDarkGray, rectCircle2);
                    g.FillEllipse(bruhBlack, rectCircle3);
                    g.DrawEllipse(penSlateGray, rectCircle2);

                    if (phaseDown != 1 && phaseDown != 2) // [!phaseDown2] could be optional
                    {
                        tickReloadStart = tickFireStart + tickLength_fireAnimation + tickLength_waitBetween;
                        phaseUp = 1;
                    }
                    break;
            }
            switch (phaseDown)
            {
                case 1:
                    tickImpactStart = lTicks;
                    break;
                case 2: // animation: impact

                    // impact crosses
                    int crossSize = 10, projectileSize = 20;
                    for (int i = 0; i < maxTargetsLocked; i++)
                    {
                        
                        g.DrawRectangle(penWhite,
                            lastBalloonPositions[i].X - crossSize, lastBalloonPositions[i].Y - crossSize,
                            crossSize*2, crossSize*2);
                        g.DrawLine(penWhite,
                            lastBalloonPositions[i].X - crossSize, lastBalloonPositions[i].Y,
                            lastBalloonPositions[i].X + crossSize, lastBalloonPositions[i].Y);
                        g.DrawLine(penWhite,
                            lastBalloonPositions[i].X, lastBalloonPositions[i].Y - crossSize,
                            lastBalloonPositions[i].X, lastBalloonPositions[i].Y + crossSize);    
                    }
                    // gray lines / earthquake thingy  [deprecated]
                    /*if (tickImpactStart + 400 < lTicks)
                        for (int i = 1; i < maxTargetsLocked; i++)
                        {
                            if (lastBalloonPositions[i].X == 0 && lastBalloonPositions[i].Y == 0) continue;
                            g.DrawLine(penBlack,
                                lastBalloonPositions[0].X, lastBalloonPositions[0].Y,
                                lastBalloonPositions[i].X, lastBalloonPositions[i].Y);
                        }
                    */
                    // falling projectile
                    if (tickImpactStart + 500 > lTicks)
                    {
                        float pathLength = 500;
                        float TS1 = TweenService(tickImpactStart,tickImpactStart+500, lastBalloonPositions[0].Y-pathLength, lastBalloonPositions[0].Y,lTicks);
                        
                        g.FillRectangle(bruhWhiteTransparent,
                            lastBalloonPositions[0].X - projectileSize, 0,
                            projectileSize*2, TS1 + projectileSize);
                        g.DrawLine(penWhite,
                            lastBalloonPositions[0].X - projectileSize, 0,
                            lastBalloonPositions[0].X - projectileSize, TS1);
                        g.DrawLine(penWhite,
                            lastBalloonPositions[0].X + projectileSize, 0,
                            lastBalloonPositions[0].X + projectileSize, TS1);
                        g.FillEllipse(bruhSlateGray,
                            lastBalloonPositions[0].X - projectileSize, TS1,
                            projectileSize * 2, projectileSize * 2);
                    }
                    
                            
                        
                    


                    if (tickImpactStart + 5000 < lTicks) phaseDown = 3;
                    break;
                case 3:
                    break;
            }


        }
        int targetIndex = -1;
        public override void Update(Game game)
        {
            ticks++;

            switch (phaseDown)
            {

                case 1: // midair
                    
                    targetIndex = game.FindTargetForTower(this);
                    if (targetIndex == -1) { break; }
                    targetCounter = 1;

                    lastBalloonPos = game.CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, game.Balloons[targetIndex].PathPosition);
                    lastBalloonPositions[0] = lastBalloonPos;
                    tickLastImpact = ticks;

                    game.DamageBalloon(targetIndex, (int)strength, this); // TODO: uint to int could be an oof conversion

                    phaseDown = 2;
                    break;
                case 2: // impact
                    if (targetCounter == maxTargetsLocked) { break; }
                    targetIndex = game.FindTargetForTower(this);
                    targetCounter++;
                    if (targetIndex == -1) { break; }
                    

                    lastBalloonPos = game.CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, game.Balloons[targetIndex].PathPosition);
                    lastBalloonPositions[targetCounter-1] = lastBalloonPos;
                    tickLastImpact = ticks;


                    game.DamageBalloon(targetIndex, (int)strength, this); // TODO: uint to int could be an oof conversion
                    break;
                case 3: // void
                    break;
            }

            //if (lastBaloonIndex != -1 && game.Balloons.Count > lastBaloonIndex) lastBalloonPos = game.CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, game.Balloons[lastBaloonIndex].PathPosition);

        }
        /// <summary>
        /// Simplifies code for animations. Linear.
        /// </summary>
        /// <param name="tickStart">At which tick to start</param>
        /// <param name="tickEnd">At which tick to end</param>
        /// <param name="minValue">Your starting output value</param>
        /// <param name="maxValue">Your final output value</param>
        /// <param name="currentTick">Current tick value</param>
        /// <returns></returns>
        private static float TweenService(ulong tickStart, ulong tickEnd, float minValue, float maxValue, ulong currentTick)
        {
            float deltaValue = maxValue - minValue, deltaTick = tickEnd-tickStart;
            float factor = Math.Min(Math.Max(currentTick - tickStart, 0), deltaTick) / deltaTick;  //  E[0;1]
            return minValue + deltaValue * factor;
        }
        private static float TweenServiceExp(ulong tickStart, ulong tickEnd, float minValue, float maxValue, ulong currentTick, float power, bool invert = false)
        {
            float deltaValue = maxValue - minValue, deltaTick = tickEnd - tickStart;
            ulong d = currentTick < tickStart ? 0 : currentTick - tickStart;
            float factor = Math.Min(d, deltaTick) / deltaTick;  //  E[0;1]
            //Console.WriteLine(factor);
            return minValue + (invert
                ? (deltaValue * (1 - (float)Math.Pow(1 - factor, power)))
                : (deltaValue * (float)Math.Pow(factor, power)));
        }

    }
}
