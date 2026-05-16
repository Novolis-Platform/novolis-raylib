using System.Runtime.InteropServices;

namespace Novolis.Raylib.Transformations;

/// <summary>3D vector (raylib <c>Vector3</c>).</summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vector3
{
    public float X;
    public float Y;
    public float Z;

    public static Vector3 Zero => default;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator *(Vector3 v, float s) => new(v.X * s, v.Y * s, v.Z * s);

    public static float Distance(Vector3 a, Vector3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static Vector3 Normalize(Vector3 v)
    {
        var len = MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        if (len <= 1e-6f)
            return new Vector3(0, 0, -1);
        return new Vector3(v.X / len, v.Y / len, v.Z / len);
    }

    public static Vector3 Cross(Vector3 a, Vector3 b) =>
        new(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

    public static Vector3 ForwardFromYawPitch(float yawRad, float pitchRad)
    {
        var cp = MathF.Cos(pitchRad);
        return Normalize(new Vector3(
            MathF.Sin(yawRad) * cp,
            MathF.Sin(pitchRad),
            -MathF.Cos(yawRad) * cp));
    }
}
