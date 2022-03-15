using System;
using Newtonsoft.Json;

namespace NoPasaranTD.Utilities
{
	[Serializable]
	[JsonObject(MemberSerialization.OptOut)]
	public struct Vector2D
	{
		public float X { get; }
		public float Y { get; }
		public Vector2D(float x, float y) { X = x; Y = y; }
		public Vector2D(double x, double y) { X = (float)x; Y = (float)y; }

		public override bool Equals(object obj) => obj is Vector2D d && X == d.X && Y == d.Y;
		public override int GetHashCode() => (X, Y).GetHashCode();
		public override string ToString() => "(" + X + '|' + Y + ')';
		
		[JsonIgnore]
		public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);

		[JsonIgnore]
		public float MagnitudeSquared => (float)(X * X + Y * Y);

		[JsonIgnore]
		public float Angle => (float)Math.Atan2(Y, X);

		public Vector2D Rotated(double angle)
		{
			double sin = Math.Sin(angle);
			double cos = Math.Cos(angle);
			return new Vector2D(cos * X - sin * Y, sin * X + cos * Y);
		}
		public Vector2D WithAngle(double angle) => Rotated(angle - Angle);
		public Vector2D WithMagnitude(double magnitude) => (magnitude / Magnitude) * this;

		public static float Intersection(Vector2D locationVA, Vector2D directionVA, Vector2D locationVB, Vector2D directionVB) => (directionVB.X * (locationVA.Y - locationVB.Y) + directionVB.Y * (locationVB.X - locationVA.X)) / (directionVA.X * directionVB.Y - directionVA.Y * directionVB.X);
		public static Vector2D FromPolar(double magnitude, double angle) => new Vector2D(magnitude * Math.Cos(angle), magnitude * Math.Sin(angle));
		
		public static Vector2D operator *(double i, Vector2D v) => new Vector2D(v.X * i, v.Y * i);
		public static Vector2D operator *(Vector2D v, double i) => new Vector2D(v.X * i, v.Y * i);
		public static Vector2D operator *(Vector2D a, Vector2D b) => new Vector2D(a.X * b.X, a.Y * b.Y);

		public static Vector2D operator /(Vector2D v, double i) => new Vector2D(v.X / i, v.Y / i);
		public static Vector2D operator /(Vector2D a, Vector2D b) => new Vector2D(a.X / b.X, a.Y / b.Y);

		public static Vector2D operator +(Vector2D a, Vector2D b) => new Vector2D(a.X + b.X, a.Y + b.Y);
		public static Vector2D operator -(Vector2D a, Vector2D b) => new Vector2D(a.X - b.X, a.Y - b.Y);
		public static Vector2D operator -(Vector2D v) => new Vector2D(-v.X, -v.Y);

		public static implicit operator Vector2D((float x, float y) t) => new Vector2D(t.x, t.y);
		public static implicit operator Vector2D((double x, double y) t) => new Vector2D(t.x, t.y);
	}
}
