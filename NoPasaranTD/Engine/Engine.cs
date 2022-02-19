using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
//using System.Windows.Drawing;

namespace NoPasaranTD
{
    public static class Engine
    {
		/// <summary>
		/// More intuitive configuration of the engine
		/// [i] Values do not exist in this form and are adjusted after INIT. You can display the pre and post values in the console via the dedicated bool.
		/// Also consider: Engine is not complete yet; there will be dynamic throttling of skipDraw and skipTick in the future
		/// </summary>
		private static class EngineConfig
		{
			public const bool useSimplifiedConfigs = true;

			public const double maxFPS = 30; // frame-updates per second
			public const double maxSPS = 1;  // simulation-ticks per second
			public const int maxEngineTicksPerSecond = 100;
			public const bool show_PrePostConfig_InConsole = true;
		}
		/// <summary>
		/// The more direct approach to configuring the engine
		/// [i] It is expected that values here can be adjusted constantly during runtime by the engine itself
		/// </summary>
		private static class EngineConfigCalculated
        {
			public static int engineTickMs;  // wait time between ticks in ms for the EngineTick
			public static int skipsForTick;  // wait X-EngineTicks every time before executing SimulationTick once
			public static int skipsForDraw;  // wait X-EngineTicks every time before executing DrawFrame once

			/// <summary>
			/// Expected to be run once during initailization phase by Form / similar
			/// </summary>
			/// <exception cref="Exception">Will throw if set to manual but no values are provided</exception>
			public static void Calculate()
            {
                #region uninteresting stuff
                if (!EngineConfig.useSimplifiedConfigs)
					if (engineTickMs == 0 || skipsForTick == 0 || skipsForDraw == 0) throw new Exception("You have to assign valid values manually!");
					else return;

				if (EngineConfig.show_PrePostConfig_InConsole)
                {
					Console.WriteLine("<> Engine Configs PRE ______");
					Console.WriteLine("maxEngineTicksPerSecond = " + EngineConfig.maxEngineTicksPerSecond.ToString());
					Console.WriteLine("maxFPS                  = " + EngineConfig.maxFPS.ToString());
					Console.WriteLine("maxSPS                  = " + EngineConfig.maxSPS.ToString());
					Console.WriteLine(" - - - - - - - - - - - - - -");
				}
				#endregion

				engineTickMs = 1000 / EngineConfig.maxEngineTicksPerSecond;
				skipsForTick = Math.Max((int)(1000 / engineTickMs / EngineConfig.maxSPS) - 1, 1);
				skipsForDraw = Math.Max((int)(1000 / engineTickMs / EngineConfig.maxFPS) - 1, 1);

                #region unintersting stuff
                if (EngineConfig.show_PrePostConfig_InConsole)
				{
					float _engineTicksPS = 1000 / engineTickMs;
					Console.WriteLine("<> Engine Configs POST ______");
					Console.WriteLine("maxEngineTicksPerSecond = " + (_engineTicksPS).ToString());
					Console.WriteLine("maxFPS                  = " + (_engineTicksPS / skipsForDraw).ToString());
					Console.WriteLine("maxSPS                  = " + (_engineTicksPS / skipsForTick).ToString());
					Console.WriteLine("_____________________________");
				}
                #endregion
            }
        }
		
        public static bool INIT()
        {
			DigitalData.CreateRandData();
			EngineConfigCalculated.Calculate();


			return true;
		}
		/// <summary>
		/// Your code for drawing stuff comes here <>
		/// </summary>
		private static void PublicDraw()
        {

			// example stuff
			Size s = new Size(9, 9);
			for (int i = 0; i < DigitalData.points.Length; i++)
				Draw.DigitalRectangle(Color.Black, DigitalData.points[i].X, DigitalData.points[i].Y, s);
			for (int i = 0; i < 200; i++)
			{
				Draw.DigitalRectangle(Color.Black, i * 10 - 1000, 0, s);
				Draw.DigitalRectangle(Color.Red, 0, i * 10 - 1000, s);
			}
			if (true) // show FPS and other stats
            {
				Font font = SystemFonts.DefaultFont; // I noticed accessing defaultfont takes a lot of cpu time
				int margin = 15;
				Point fpsPos = new Point(10, 10);
				Draw.UIRectangle(Color.LightGray, fpsPos.X, fpsPos.Y, new Size(100, margin * 5), false, false);
				Draw.UIText("ETPS: " + Info.GetETPS().ToString(), font, Brushes.Black, fpsPos.X, fpsPos.Y);
				Draw.UIText("FPS: " + Info.GetFPS().ToString(), font, Brushes.Black, fpsPos.X, fpsPos.Y + margin * 1);
				Draw.UIText("SPS: " + Info.GetSPS().ToString(), font, Brushes.Black, fpsPos.X, fpsPos.Y + margin * 2);
				Draw.UIText("EngineTickMs(real)    : " + Info.GetEngineTickMsTime().ToString(), font, Brushes.Black, fpsPos.X, fpsPos.Y + margin * 3);
				Draw.UIText("EngineTickMs(expected): " + EngineConfigCalculated.engineTickMs.ToString(), font, Brushes.Black, fpsPos.X, fpsPos.Y + margin * 4);
			}
			


		}
		/// <summary>
		/// Your goto class for drawing on the canvas\n
		/// [i] Methods with prefix "Digital" are reffereing to everything to be drawen as object of the game world<br>
		/// [i] Methods with prefix "UI" are reffereing to everything to be drawen as UI
		/// </summary>
		private static class Draw
        {
			/// <summary>
			/// Draw game world objects on the canvas as rectangle
			/// </summary>
			public static void DigitalRectangle(Color color, int posX, int posY, Size size, bool sizeX_IsCentralized = true, bool sizeY_IsCentralized = true)
            {
				INTERNAL.DirectDraw.Rect(color, posX, posY, size, sizeX_IsCentralized, sizeY_IsCentralized);
            }
			/// <summary>
			/// will probably be deprecated or modified in some way
			/// </summary>
			private static void RectangleF(Color color, int posX, int posY, Size size, bool sizeX_IsCentralized = true, bool sizeY_IsCentralized = true)
			{
				INTERNAL.DirectDraw.RectF(color, posX, posY, size, sizeX_IsCentralized, sizeY_IsCentralized);
			}
			/// <summary>
			/// Draw your UI elements directly on the canvas, as rectangle
			/// </summary>
			public static void UIRectangle(Color color, int posX, int posY, Size size, bool sizeX_IsCentralized = true, bool sizeY_IsCentralized = true)
            {
				INTERNAL.DirectDraw.RectOnCanvas(color, posX, posY, size, sizeX_IsCentralized, sizeY_IsCentralized);
            }
			public static void UIText(string txt, Font font, Brush brush,int posX, int posY, bool sizeX_IsCentralized = false, bool sizeY_IsCentralized = false)
			{
				INTERNAL.DirectDraw.TextOnCanvas(txt, font, brush, posX, posY, sizeX_IsCentralized, sizeY_IsCentralized);
			}
		}

		private static class Info
        {
			public static int GetETPS()
			{
				return (int)INTERNAL.lastSec_ETPS;
			}
			public static int GetSPS()
			{
				return (int)INTERNAL.lastSec_SPS;
			}
			public static int GetFPS()
            {
				return (int)INTERNAL.lastSec_FPS;
            }
			public static double GetEngineTickMsTime()
            {
				return INTERNAL.EngineTick_MsTime;
            }
        }
		/// <summary>
		/// Key components for communication between Engine and FormDisplay.
		/// These should generally not be touched by non engine stuff; use the other accessables provided via "Engine.<###>" instead.
		/// </summary>
		public static class INTERNAL
        {
            #region internal code pieces
            public static Form FormDisplay = null;
			public static bool newResize = false;
			public static bool DOupdateCanvas = false;
			private static Graphics G;
			public static uint lastSec_ETPS = 0; // updated constantly during runtime
			public static uint lastSec_SPS = 0;  // " "
			public static uint lastSec_FPS = 0;  // " "
			public static double EngineTick_MsTime = 0; // " "

			public static void THREADEDLoop()
			{
				int skipsDraw = EngineConfigCalculated.skipsForDraw;
				int skipsTick = EngineConfigCalculated.skipsForTick;
				int engineTickMs = EngineConfigCalculated.engineTickMs;
				uint iteration = 0;
				uint counter_ETPS = 0;
				uint counter_SPS = 0;
				uint counter_FPS = 0;
				DateTime dtPS = DateTime.Now; // NOTE: maybe could be replaced by something better
				DateTime dt = DateTime.Now;   // " "
				while (true)
				{
					 
					EngineTick_MsTime = (DateTime.Now - dt).Milliseconds;
					dt = DateTime.Now;
					if (dt > dtPS)
                    {
						dtPS = DateTime.Now.AddSeconds(1);
						lastSec_ETPS = counter_ETPS; lastSec_FPS = counter_FPS; lastSec_SPS = counter_SPS;
						counter_ETPS = 0; counter_FPS = 0; counter_SPS = 0;
					}
					Thread.Sleep(engineTickMs);

					counter_ETPS++;
					if (INTERNAL.newResize) ; // do nothing for now

					if (iteration % skipsDraw == 0)
					{
						INTERNAL.DOupdateCanvas = true;
						counter_FPS++;
					}

					if (iteration % skipsTick == 0)
					{
						long pretime = DateTime.Now.Ticks;
						DoTick();
						long posttime = DateTime.Now.Ticks;
						counter_SPS++;
					}

					iteration++;
				}

			}

			/// <summary>
			/// Renders one frame
			/// </summary>
			public static void DoDraw(object sender, PaintEventArgs e)
			{
				#region Update Values
				G = e.Graphics;
				ValueStack.windX = e.ClipRectangle.Width;
				ValueStack.windY = e.ClipRectangle.Height;
				#endregion
				PublicDraw();
			}
			/// <summary>
			/// Calculates one tick of physics/simulation
			/// </summary>
			public static void DoTick()
			{
				// hier kommt die verknüpfung zum simulations zeug
				
				//   \/ zeug einfach nur zum testen
				for (int i = 0; i < DigitalData.points.Length; i++)
				
					DigitalData.points[i] = new Point(DigitalData.rand.Next(2001) - 1000, DigitalData.rand.Next(2001) - 1000);
				
			}

			// DrawLib is deprecated ; will probably be obselete and therefore removed in the future
			private static class DrawLib
			{
				#region Converter Tools / Methods
				public static Rectangle ToScreenRect(int posX, int posY, Size size, bool sizeX_IsCentralized = true, bool sizeY_IsCentralized = true)
				{
					double digitalX = ValueStack.cameraPosX - posX, digitalY = ValueStack.cameraPosY - posY;
					double
						endSizeX = size.Width * ValueStack.scaleX,
						endSizeY = size.Height * ValueStack.scaleY;
					double
						endPosX = ValueStack.windX / 2 - digitalX * ValueStack.scaleX,
						endPosY = ValueStack.windY / 2 + digitalY * ValueStack.scaleY;
					return new Rectangle(
						(int)(endPosX - (sizeX_IsCentralized ? endSizeX / 2 : 0)),
						(int)(endPosY - (sizeY_IsCentralized ? endSizeY / 2 : 0)),
						(int)endSizeX, (int)endSizeY
						);
				}
				public static RectangleF ToScreenRectF(int posX, int posY, Size size, bool sizeX_IsCentralized = true, bool sizeY_IsCentralized = true)
				{
					double digitalX = ValueStack.cameraPosX - posX, digitalY = ValueStack.cameraPosY - posY;
					double
						endSizeX = size.Width * ValueStack.scaleX,
						endSizeY = size.Height * ValueStack.scaleY;
					double
						endPosX = ValueStack.windX / 2 - digitalX * ValueStack.scaleX,
						endPosY = ValueStack.windY / 2 + digitalY * ValueStack.scaleY;
					return new RectangleF(
						(float)(endPosX - (sizeX_IsCentralized ? endSizeX / 2 : 0)),
						(float)(endPosY - (sizeY_IsCentralized ? endSizeY / 2 : 0)),
						(float)endSizeX, (float)endSizeY
						);
				}
				public static Size ToScreenSize(double sizeX = 0, double sizeY = 0)
				{
					return new Size((int)(sizeX * ValueStack.scaleX), (int)(sizeY * ValueStack.scaleY));
				}
				public static SizeF ToScreenSizeF(double sizeX = 0, double sizeY = 0)
				{
					return new SizeF((float)sizeX * ValueStack.scaleX, (float)sizeY * ValueStack.scaleY);
				}
				// same thing as ToScreenRect, but with removed size parameter
				public static Point ToScreenPoint(int posX, int posY)
				{
					double digitalX = ValueStack.cameraPosX - posX, digitalY = ValueStack.cameraPosY - posY;
					double
						endPosX = ValueStack.windX / 2 - digitalX * ValueStack.scaleX,
						endPosY = ValueStack.windY / 2 + digitalY * ValueStack.scaleY;
					return new Point(
						(int)(endPosX),
						(int)(endPosY)
						);
				}
				public static Point ToDigitalPoint(int posX, int posY)
				{
					double
						endPosX = (-ValueStack.windX / 2 + posX) / ValueStack.scaleX + ValueStack.cameraPosX,
						endPosY = (-ValueStack.windY / 2 + posY) / (-ValueStack.scaleY) + ValueStack.cameraPosY;
					return new Point(
						(int)endPosX,
						(int)endPosY
						);
				}
				public static double GetVectLength(Point a)
				{
					return Math.Sqrt(a.X * a.X + a.Y * a.Y);
				}
				public static double GetVectLength(double x, double y, double z = 0)
				{
					return Math.Sqrt(x * x + y * y + z * z);
				}
				#endregion
			}
			public static class DirectDraw
			{
				#region Drawing Tools / Methods
				public static void Rect(Color color, int posX, int posY, Size size, bool sizeX_IsCentralized, bool sizeY_IsCentralized)
				{
					double digitalX = ValueStack.cameraPosX - posX, digitalY = ValueStack.cameraPosY - posY;
					double
						endSizeX = size.Width * ValueStack.scaleX,
						endSizeY = size.Height * ValueStack.scaleY;
					double
						endPosX = ValueStack.windX / 2 - digitalX * ValueStack.scaleX,
						endPosY = ValueStack.windY / 2 + digitalY * ValueStack.scaleY;
					G.FillRectangle(new SolidBrush(color), new Rectangle(
						(int)(endPosX - (sizeX_IsCentralized ? endSizeX / 2 : 0)),
						(int)(endPosY - (sizeY_IsCentralized ? endSizeY / 2 : 0)),
						(int)endSizeX, (int)endSizeY
						));
				}
				public static void RectF(Color color, int posX, int posY, Size size, bool sizeX_IsCentralized, bool sizeY_IsCentralized)
				{
					double digitalX = ValueStack.cameraPosX - posX, digitalY = ValueStack.cameraPosY - posY;
					double
						endSizeX = size.Width * ValueStack.scaleX,
						endSizeY = size.Height * ValueStack.scaleY;
					double
						endPosX = ValueStack.windX / 2 - digitalX * ValueStack.scaleX,
						endPosY = ValueStack.windY / 2 + digitalY * ValueStack.scaleY;
					G.FillRectangle(new SolidBrush(color), new RectangleF(
						(float)(endPosX - (sizeX_IsCentralized ? endSizeX / 2 : 0)),
						(float)(endPosY - (sizeY_IsCentralized ? endSizeY / 2 : 0)),
						(float)endSizeX, (float)endSizeY
						));
				}
				/// <summary>
				/// NOTE: still in development TODO: make it more intuitive, accessable and powerful
				/// </summary>
				public static void RectOnCanvas(Color color, int posX, int posY, Size size, bool sizeX_IsCentralized, bool sizeY_IsCentralized)
				{
					G.FillRectangle(new SolidBrush(color), new Rectangle(
						(int)(posX - (sizeX_IsCentralized ? size.Width / 2 : 0)),
						(int)(posY - (sizeY_IsCentralized ? size.Height / 2 : 0)),
						size.Width, size.Height
						));
				}
				/// <summary>
				/// NOTE: still in development TODO: make it more intuitive, accessable and powerful
				/// </summary>
				public static void TextOnCanvas(string txt, Font font, Brush brush, int posX, int posY, bool sizeX_IsCentralized, bool sizeY_IsCentralized)
				{
					G.DrawString(txt, font, brush, new Point(
						(int)(posX - (sizeX_IsCentralized ? G.MeasureString(txt, font).Width / 2 : 0)),
						(int)(posY - (sizeY_IsCentralized ? G.MeasureString(txt, font).Height / 2 : 0))
						));
				}
				#endregion
			}

			// TODO: overall class integrity could be improved by merging this one with something
			private static class ValueStack
			{
				public static float scaleX = 1f, scaleY = 1f;
				public static int windX = 0, windY = 0;
				public static int cameraPosX = 0, cameraPosY = 0;
				public static Point mouseMoveVect = new Point(0, 0);
				public static Point mouse = new Point(0, 0);
				
			}
			#endregion
		}

	}
	// class for testing purposes
	static class DigitalData
    {
		public static Point[] points;
		public static Random rand = new Random(0);
		public static void CreateRandData()
        {
			points = new Point[100];
            for (int i = 0; i < points.Length; i++)
            {
				points[i] = new Point(rand.Next(2001)-1000, rand.Next(2001)-1000);
            }
        }
    }
}
