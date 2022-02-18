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
		/// </summary>
		private static class EngineConfig
		{
			public const bool _useSimplifiedConfigs = true;

			public const int _maxEngineTicksPerSecond = 100;
			public const bool _show_PrePostConfig_InConsole = true; // TODO: implementation
		}
		/// <summary>
		/// The more manly approach to configuring the engine!
		/// [i] It is expected that values here can be adjusted constantly during runtime by the engine itself
		/// </summary>
		private static class EngineConfigCalculated
        {
			public static int _engineTickMs;  // wait time between ticks in ms for the engine-tick

			/// <summary>
			/// Expected to be run once during initailization phase by Form / similar
			/// </summary>
			/// <exception cref="Exception">Will throw if set to manual but no values are provided</exception>
			public static void Calculate()
            {
				if (!EngineConfig._useSimplifiedConfigs)
					if (_engineTickMs == 0) throw new Exception("You have to assign valid values manually!");
					else return;

				if (EngineConfig._show_PrePostConfig_InConsole)
                {
					Console.WriteLine("<> Engine Configs PRE ______");
					Console.WriteLine("_maxEngineTicksPerSecond = "+EngineConfig._maxEngineTicksPerSecond.ToString());
					//Console.WriteLine("   " + EngineConfig. .ToString());
					Console.WriteLine(" - - - - - - - - - - - - - -");
				}


				_engineTickMs = 1000 / EngineConfig._maxEngineTicksPerSecond;


				if (EngineConfig._show_PrePostConfig_InConsole)
				{
					Console.WriteLine("<> Engine Configs POST ______");
					Console.WriteLine("_maxEngineTicksPerSecond = " + (EngineConfigCalculated._engineTickMs * EngineConfig._maxEngineTicksPerSecond).ToString());
					Console.WriteLine("_____________________________");
				}

			}
		}
		
        public static bool INIT()
        {
			DigitalData.CreateRandData();
			EngineConfigCalculated.Calculate();


			return true;
		}


		

		/// <summary>
		/// Do not touch these thingies, they are too sensitve UwU
		/// Key components for communication between Engine and FormDisplay.
		/// These should generally not be touched by non engine stuff; use the other accessables provided via "Engine.<###>" instead.
		/// </summary>
		public static class INTERNAL
        {
            #region internal code pieces
            public static Form FormDisplay = null;
			public static bool newResize = false;
			public static bool DOupdateCanvas = false;

			public static void THREADEDLoop()
			{
				int skipsDraw = 20;
				int skipsTick = 10;
				uint iteration = 0;
				while (true)
				{
					Thread.Sleep(EngineConfig._maxEngineTicksPerSecond);
					if (INTERNAL.newResize) ; // do nothing for now

					if (iteration % skipsDraw == 0)
					{
						INTERNAL.DOupdateCanvas = true;
					}

					if (iteration % skipsTick == 0)
					{
						long pretime = DateTime.Now.Ticks;
						DoTick();
						long posttime = DateTime.Now.Ticks;
					}

					iteration++;
				}

			}


			public static void DoDraw(object sender, PaintEventArgs e)
			{
				Graphics G = e.Graphics;
				#region Update Values
				ValueStack.windX = e.ClipRectangle.Width;
				ValueStack.windY = e.ClipRectangle.Height;
				#endregion

				Size s = new Size(9, 9);
				SolidBrush sb = new SolidBrush(Color.Black);
				SolidBrush sbRed = new SolidBrush(Color.Red);
				for (int i = 0; i < DigitalData.points.Length; i++)
				{
					G.FillRectangle(sb, DrawLib.ToScreenRect(DigitalData.points[i].X, DigitalData.points[i].Y, s));
				}
				for (int i = 0; i < 200; i++)
				{
					G.FillRectangle(sbRed, DrawLib.ToScreenRect(i * 10 - 1000, 0, s));
					G.FillRectangle(sbRed, DrawLib.ToScreenRect(0, i * 10 - 1000, s));
				}

			}
			private static Random rand2 = new Random(0);
			public static void DoTick()
			{
				// simulations zeug verknüpfung
				for (int i = 0; i < DigitalData.points.Length; i++)
				{
					DigitalData.points[i] = new Point(rand2.Next(2001) - 1000, rand2.Next(2001) - 1000);
				}
			}

			// TODO: Split class into "DrawTools" and "DrawToScreen" with void return and deeper Graphics intergration
			private static class DrawLib
			{
				#region Drawing Tools / Methods
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
			// TODO: overall class integrity could be improved by merging this one with something
			private static class ValueStack
			{
				public static double _globalStepFactor = 1; // constant after load
				public static float scaleX = 1f, scaleY = 1f, fpsReal = 0;
				public static int fpsFrames = 0, fpsSecond = 0;
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
		private static Random rand = new Random(0);
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
