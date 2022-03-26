using NoPasaranTD.Engine;
using System;
using System.Drawing;

namespace NoPasaranTD.Model
{
    [Serializable]
    public class TowerArtillery : Tower
    {
        private static readonly Pen RANGE_CIRCLE_PEN = new Pen(Color.Purple, 2.3f);

        Utilities.Vector2D lastBalloonPos;
        Utilities.Vector2D[] lastBalloonPositions;
        Utilities.Vector2D rotationVec = new Utilities.Vector2D(30,30), vect = new Utilities.Vector2D(0,0);
        PointF centerCanon = new Point(0,0);
        RectangleF rectPipeTransformed = new RectangleF(), rectCircle1 = new RectangleF(), rectCircle2 = new RectangleF(),
            rectCircle3 = new RectangleF(), rectPipe = new RectangleF();

        //animation related
        int phaseUp = 1; // 1:reloading | 2:firing | 3:waiting
        int phaseDown = 3; // 1:midair | 2:impact | 3:void
        float ratioPipeToHitbox;
        int targetCounter = 0;

        ulong ticks = 0; // Anzahl vergangener Ticks
        ulong tickReloadStart = 0;
        ulong tickFireStart = 0;
        ulong tickImpactStart = 0;

        ulong tickLength_fireAnimation = 2000;
        ulong tickLength_waitBetweenReloadAndFire = 1000;
        ulong tickLength_impactAnimation = 1000;
        ulong tickLength_impactCrossFade = 1500;
        bool reloadOnlyAfterImpact = true;
        int maxTargetsLocked = 10;

        public TowerArtillery()
        {
            TargetMode = TowerTargetMode.Farthest;
            lastBalloonPositions = new Utilities.Vector2D[maxTargetsLocked];
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

            /*
             *             // Zeichnet die Hitbox des Towers
            if (IsPositionValid || IsPlaced) // Der Tower wird normal gezeichnet wenn dieser gesetzt ist oder seine Position valide ist
                g.FillPolygon(Brushes.SlateGray, hitboxPolygonCorners);
            else if (!IsPlaced) // Ist der Tower nicht gesetzt und die Position ist nicht Valide dann soll dieser einen roten Ground haben
                g.FillPolygon(Brushes.Red, hitboxPolygonCorners);
            */

            // Bei Hindernissen wird es rot gezeichnet

            if (IsPositionValid || IsPlaced)
            {
                g.FillRectangle(Brushes.DarkGray, leg1);
                g.FillRectangle(Brushes.DarkGray, leg2);
                g.DrawRectangle(Pens.SlateGray, leg1.X, leg1.Y, leg1.Width, leg1.Height);
                g.DrawRectangle(Pens.SlateGray, leg2.X, leg2.Y, leg2.Width, leg2.Height);
                g.FillRectangle(Brushes.Orange, supportLeg1);
                g.FillRectangle(Brushes.Orange, supportLeg2);
            }
            else if (!IsPlaced)
            {
                int lines = 40;
                float lineStep = (Hitbox.Width + Hitbox.Height) / (float)lines;
                for (int i = 1; i < lines + 1; i++)
                    g.DrawLine(Pens.DarkRed,
                       Math.Min(Hitbox.Left + lineStep * i, Hitbox.Right),
                       Hitbox.Top + Math.Max(Hitbox.Left + lineStep * i - Hitbox.Right, 0),
                       Hitbox.Left + Math.Max(Hitbox.Top + lineStep * i - Hitbox.Bottom, 0),
                       Math.Min(Hitbox.Top + lineStep * i, Hitbox.Bottom)
                       );
                g.FillRectangle(new SolidBrush(Color.FromArgb(20, Color.Red)), Hitbox);

                g.FillRectangle(Brushes.Red, leg1);
                g.FillRectangle(Brushes.Red, leg2);
                g.DrawRectangle(Pens.Red, leg1.X, leg1.Y, leg1.Width, leg1.Height);
                g.DrawRectangle(Pens.Red, leg2.X, leg2.Y, leg2.Width, leg2.Height);
                g.FillRectangle(Brushes.Red, supportLeg1);
                g.FillRectangle(Brushes.Red, supportLeg2);
            }
            


            // draws the time left to the next shot in the corner of the tower | generally a debugging/visualization thingy
            //g.DrawString((delay - lTicks + tickReloadStart).ToString(), font, bruhBlack, Hitbox.Location);


            // attack radius
            if (IsSelected) g.DrawEllipse(RANGE_CIRCLE_PEN, Hitbox.X + (int)(Hitbox.Width / 2.0 - Range), Hitbox.Y + (int)(Hitbox.Height / 2 - Range), (int)Range * 2, (int)Range * 2);

            ulong tickLength_reloadAnimation = Delay
                - tickLength_fireAnimation
                - tickLength_waitBetweenReloadAndFire
                - (reloadOnlyAfterImpact ? Math.Max(tickLength_impactAnimation, tickLength_impactCrossFade) : 0);

            switch (phaseUp)
            {
                case 1: // animation: reload
                    
                    PointF margin = new PointF(Hitbox.Width*(1-ratioPipeToHitbox)/2, Hitbox.Height*(1-ratioPipeToHitbox)/2);
                    rectPipe = new RectangleF(Hitbox.X+margin.X, Hitbox.Y+margin.Y, Hitbox.Width * ratioPipeToHitbox, Hitbox.Height * ratioPipeToHitbox);
                    float TS1 = TweenService(tickReloadStart, tickReloadStart + tickLength_reloadAnimation, 0, diameter/2, lTicks); // für die linke seite
                    float TS2 = TweenServiceExp(tickReloadStart, tickReloadStart + tickLength_reloadAnimation, 0, rectPipe.Width - TS1 - diameter / 2, lTicks, 6); // für die rechte seite
                    
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
                    
                    

                    // Bei Hindernissen wird es rot gezeichnet
                    if (IsPositionValid || IsPlaced)
                    {
                        g.FillEllipse(Brushes.DarkGray, rectCircle1);
                        g.DrawEllipse(Pens.SlateGray, rectCircle1);
                        g.FillRectangle(Brushes.DarkGray, rectPipeTransformed);
                        g.DrawLine(Pens.SlateGray, leftLineX, rectPipe.Y, rightLineX, rectPipe.Y);
                        g.DrawLine(Pens.SlateGray, leftLineX, rectPipe.Bottom, rightLineX, rectPipe.Bottom);
                        g.FillEllipse(Brushes.DarkGray, rectCircle2);
                        g.FillEllipse(Brushes.Black, rectCircle3);
                        g.DrawEllipse(Pens.SlateGray, rectCircle2);
                    }
                    else if (!IsPlaced)
                    {
                        g.FillEllipse(Brushes.Red, rectCircle1);
                        g.DrawEllipse(Pens.Red, rectCircle1);
                        g.FillRectangle(Brushes.Red, rectPipeTransformed);
                        g.DrawLine(Pens.Red, leftLineX, rectPipe.Y, rightLineX, rectPipe.Y);
                        g.DrawLine(Pens.Red, leftLineX, rectPipe.Bottom, rightLineX, rectPipe.Bottom);
                        g.FillEllipse(Brushes.Red, rectCircle2);
                        g.FillEllipse(Brushes.Red, rectCircle3);
                        g.DrawEllipse(Pens.Red, rectCircle2);
                    }

                    


                    

                    centerCanon = new PointF(leftLineX,upperY + (lowerY-upperY)/2);

                    if (tickReloadStart + tickLength_reloadAnimation < lTicks) { tickFireStart = tickReloadStart + tickLength_reloadAnimation + tickLength_waitBetweenReloadAndFire; phaseUp = 2; }
                    break;
                case 2: // animation: fire!!
                    if (phaseDown != 1 && phaseDown != 2) // [!phaseDown2] could be optional
                    {
                        // base canon
                        g.FillRectangle(Brushes.DarkGray, rectPipeTransformed);
                        g.FillEllipse(Brushes.DarkGray, rectCircle1);
                        g.FillEllipse(Brushes.DarkGray, rectCircle2);
                        g.FillEllipse(Brushes.Black, rectCircle3);
                        g.DrawEllipse(Pens.SlateGray, rectCircle2);

                        // blast particles
                        if (ticks % 2 == 0 && lTicks > tickFireStart + tickLength_fireAnimation * 0.1)
                        {
                            rotationVec = rotationVec.Rotated(1);
                            g.DrawLine(Pens.Red, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);
                            rotationVec = rotationVec.Rotated(45);
                            g.DrawLine(Pens.Red, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);
                            rotationVec = rotationVec.Rotated(45);
                            g.DrawLine(Pens.Red, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);
                            rotationVec = rotationVec.Rotated(45);
                            g.DrawLine(Pens.Red, centerCanon.X - rotationVec.X, centerCanon.Y - rotationVec.Y, centerCanon.X + rotationVec.X, centerCanon.Y + rotationVec.Y);
                        }
                        // projectile
                        
                        float TS3 = TweenServiceExp(tickFireStart, tickFireStart + tickLength_fireAnimation,20,100,lTicks,7);
                        float TS4 = TweenServiceExp(tickFireStart+ (ulong)(tickLength_fireAnimation*0.8f), tickFireStart + tickLength_fireAnimation, 255, 0, lTicks, 2);
                        SolidBrush bruhTransparent = new SolidBrush(Color.FromArgb((int)TS4, Color.SlateGray));
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
                    g.FillRectangle(Brushes.DarkGray, rectPipeTransformed);
                    g.FillEllipse(Brushes.DarkGray, rectCircle1);
                    g.FillEllipse(Brushes.DarkGray, rectCircle2);
                    g.FillEllipse(Brushes.Black, rectCircle3);
                    g.DrawEllipse(Pens.SlateGray, rectCircle2);

                    if (phaseDown != 1 && ( !reloadOnlyAfterImpact || phaseDown != 2)) // [!phaseDown2] could be optional
                    {
                        tickReloadStart = lTicks;
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


                    // impact ground wave
                    ulong groundWaveTime = (ulong)(tickLength_impactAnimation * 4.0 / 5);
                    float blastRadius = 200;
                    if (tickImpactStart + groundWaveTime < lTicks)
                    {
                        float TS12 = TweenServiceExp(tickImpactStart + groundWaveTime, tickImpactStart + tickLength_impactAnimation, 1, 0, lTicks, 2, false);
                        SolidBrush bruhTransparent = new SolidBrush(Color.FromArgb((int)(TS12*150), Color.DarkGray));
                        g.FillEllipse(bruhTransparent,
                                lastBalloonPositions[0].X - blastRadius * (1 - TS12),
                                lastBalloonPositions[0].Y - blastRadius * (1 - TS12),
                                blastRadius * 2 * (1 - TS12), blastRadius * 2 * (1-TS12));
                    }

                    // impact crosses
                    if (tickImpactStart + tickLength_impactCrossFade > lTicks)
                    {
                        int crossSize = 10;
                        float TS14 = TweenServiceExp(tickImpactStart, tickImpactStart + tickLength_impactCrossFade, 1, 0, lTicks, 1);
                        Pen penTransparent = new Pen(Color.FromArgb((int)Math.Max(TS14*255,10), Color.White));
                        SolidBrush bruhTransparent = new SolidBrush(Color.FromArgb((int)Math.Max(TS14*100,5), Color.White));
                        for (int i = 0; i < realTargetCounter; i++)
                        {
                            g.FillEllipse(bruhTransparent,
                                lastBalloonPositions[i].X - crossSize*2, lastBalloonPositions[i].Y - crossSize*2,
                                crossSize * 4, crossSize * 4);
                            g.DrawEllipse(penTransparent,
                                lastBalloonPositions[i].X - crossSize, lastBalloonPositions[i].Y - crossSize,
                                crossSize * 2, crossSize * 2);
                            g.DrawLine(penTransparent,
                                lastBalloonPositions[i].X - crossSize, lastBalloonPositions[i].Y,
                                lastBalloonPositions[i].X + crossSize, lastBalloonPositions[i].Y);
                            g.DrawLine(penTransparent,
                                lastBalloonPositions[i].X, lastBalloonPositions[i].Y - crossSize,
                                lastBalloonPositions[i].X, lastBalloonPositions[i].Y + crossSize);
                            
                        }
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
                    if (tickImpactStart + tickLength_impactAnimation > lTicks)
                    {
                        // Vector Balloon to Canon
                        vect = new Utilities.Vector2D(
                            (rectPipe.X + diameter / 2)-lastBalloonPositions[0].X,
                            (rectPipe.Y + diameter / 2)-lastBalloonPositions[0].Y
                        );
                        // draw line between canon center and target / debugging thingy
                        //g.DrawLine(penBlack, lastBalloonPositions[0].X + vect.X, lastBalloonPositions[0].Y + vect.Y,
                        //    lastBalloonPositions[0].X, lastBalloonPositions[0].Y);
                        vect = vect.WithMagnitude(vect.Magnitude * 0.2f); // cutting the vector down to 20% --> this is then the trajectory vector
                        float TS1 = TweenService(tickImpactStart, tickImpactStart + tickLength_impactAnimation, 1, 0, lTicks);
                        float TS3 = TweenService(tickImpactStart, tickImpactStart + tickLength_impactAnimation, 100, 20, lTicks);
                        float TS4 = TweenServiceExp(tickImpactStart, tickImpactStart + tickLength_impactAnimation, 0, 255, lTicks, 2, true);

                        SolidBrush bruhTransparent = new SolidBrush(Color.FromArgb((int)TS4, Color.SlateGray));
                        g.FillEllipse(bruhTransparent,
                            lastBalloonPositions[0].X + vect.X * TS1 - TS3, lastBalloonPositions[0].Y + vect.Y * TS1 - TS3,
                            TS3 * 2, TS3 * 2);
                        
                        // super sonic sound/air waves
                        float sonicBoomSize = 100;
                        ulong layerTimeToLive = 500;
                        int layerAmount = 20;
                        int transparencyValue = 200;
                        ulong layersFinalEndTime = (ulong)(tickLength_impactAnimation*0.8);
                        int initalTransparency = Math.Max(Math.Min(transparencyValue / layerAmount,255),0);
                        for (int i = 0; i < layerAmount; i++)
                        {
                            ulong
                                layerStart = tickImpactStart + (ulong)(
                                (double)layersFinalEndTime * i / layerAmount),
                                layerEnd   = tickImpactStart + (ulong)(
                                (double)layersFinalEndTime * i / layerAmount + layerTimeToLive);
                            float TS_fadeLayer = TweenService(layerStart, layerEnd, initalTransparency, 0, lTicks);
                            float TS_constP= TweenService(tickImpactStart, tickImpactStart + tickLength_impactAnimation, 1, 0, layerStart);
                            
                            float TS3_projectileSizeAtT = TweenService(tickImpactStart, tickImpactStart + tickLength_impactAnimation, 100, 20, layerStart);
                            float TS1_fixed = TweenService(tickImpactStart, tickImpactStart + tickLength_impactAnimation, 1, 0, layerStart);
                            float lSonicBoomSize = sonicBoomSize * TS1_fixed + TS3_projectileSizeAtT * (1 - TS1_fixed);
                            bruhTransparent = new SolidBrush(Color.FromArgb((int)TS_fadeLayer, Color.White));
                            g.FillEllipse(bruhTransparent,
                            lastBalloonPositions[0].X + vect.X * TS_constP - lSonicBoomSize,
                            lastBalloonPositions[0].Y + vect.Y * TS_constP - lSonicBoomSize,
                            lSonicBoomSize * 2, lSonicBoomSize * 2);
                        }
                    }
                    
                    if (tickImpactStart + tickLength_impactCrossFade < lTicks
                        && tickImpactStart + tickLength_impactAnimation < lTicks) phaseDown = 3;
                    break;
                case 3:
                    break;
            }


        }
        (int segment, int index) target = (-1, -1);
        int realTargetCounter = 0;
        public override void Update(Game game)
        {
            ticks++;

            switch (phaseDown)
            {

                case 1: // midair
                    
                    target = game.FindTargetForTower(this);
                    if (target.index == -1) { break; }
                    // targetCounter --> total tries of searchin target; realTargetCounter --> total unique targets found
                    targetCounter = 1; realTargetCounter = 1;

                    lastBalloonPos = game.CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, game.Balloons[target.segment][target.index].PathPosition);
                    lastBalloonPositions[0] = lastBalloonPos;

                    game.DamageBalloon(target.segment, target.index, (int)Strength, this); // TODO: uint to int could be an oof conversion

                    phaseDown = 2;
                    break;
                case 2: // impact
                    if (targetCounter == maxTargetsLocked) { break; }
                    target = game.FindTargetForTower(this);
                    targetCounter++;
                    if (target.index == -1) { break; }
                    

                    lastBalloonPos = game.CurrentMap.GetPathPosition(StaticEngine.RenderWidth, StaticEngine.RenderHeight, game.Balloons[target.segment][target.index].PathPosition);
                    /* 
                    bool b = false;
                    for (int i = 1; i < realTargetCounter; i++)
                    {
                        Utilities.Vector2D v = lastBalloonPositions[i - 1];
                        if (v.X == lastBalloonPos.X && v.Y == lastBalloonPos.Y) { b = true; break; } 
                    }
                    if (b) break;
                    */
                    lastBalloonPositions[realTargetCounter-1] = lastBalloonPos;
                    
                    realTargetCounter++;

                    game.DamageBalloon(target.segment, target.index, (int)Strength, this); // TODO: uint to int could be an oof conversion
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
        /// <summary>
        /// Simplifies code for animations. Exponential. <br />
        /// --> x^y , xE[0;1] <br />
        /// Regarding "invert", you can find a good visualization of similar concepts here <br />
        /// https://developer.roblox.com/en-us/api-reference/enum/EasingStyle
        /// </summary>
        /// <param name="tickStart"></param>
        /// <param name="tickEnd"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="currentTick"></param>
        /// <param name="power"></param>
        /// <param name="invert"></param>
        /// <returns></returns>
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

        public override string ToString() => "Artillery";
    }
}
