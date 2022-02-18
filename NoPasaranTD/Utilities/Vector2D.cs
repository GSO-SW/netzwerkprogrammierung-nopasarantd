using System;
using Newtonsoft.Json;

namespace NoPasaranTD.Utilities
{
	public struct Vector2D
	{
		public float X { get; set; }
		public float Y { get; set; }
		public Vector2D(float x, float y) { X = x; Y = y; }
		public Vector2D(double x, double y) { X = (float)x; Y = (float)y; }

		public override int GetHashCode() => (X, Y).GetHashCode();
		public override bool Equals(object obj) => obj is Vector2D v0 && X == v0.X && Y == v0.Y;
		public override string ToString() => "(" + X + '|' + Y + ')';
 
		public double Magnitude => Math.Sqrt(X * X + Y * Y);

		public static Vector2D operator *(double i, Vector2D v) => new Vector2D(v.X * i, v.Y * i);
		public static Vector2D operator *(Vector2D v, double i) => new Vector2D(v.X * i, v.Y * i);
		public static Vector2D operator *(Vector2D a, Vector2D b) => new Vector2D(a.X * b.X, a.Y * b.Y);

		public static Vector2D operator /(Vector2D v, double i) => new Vector2D(v.X / i, v.Y / i);
		public static Vector2D operator /(Vector2D a, Vector2D b) => new Vector2D(a.X / b.X, a.Y / b.Y);

		public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
		public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);

		public static implicit operator Vector2D((float x, float y) t) => new Vector2D(t.x, t.y);
		public static implicit operator Vector2D((double x, double y) t) => new Vector2D(t.x, t.y);
	}
}
