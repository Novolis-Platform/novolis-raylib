using System.Numerics;

namespace Novolis.Raylib;

/// <summary>raylib-oriented helpers for <see cref="Vector3"/>.</summary>
public static class RaylibVector3
{
    /// <summary>Unit forward vector from yaw and pitch in radians (Y-up, −Z forward at yaw 0).</summary>
    /// <param name="yawRad">Horizontal rotation in radians.</param>
    /// <param name="pitchRad">Vertical rotation in radians.</param>
    /// <returns>Normalized direction vector.</returns>
    public static Vector3 ForwardFromYawPitch(float yawRad, float pitchRad)
    {
        var cp = MathF.Cos(pitchRad);
        return Vector3.Normalize(new Vector3(
            MathF.Sin(yawRad) * cp,
            MathF.Sin(pitchRad),
            -MathF.Cos(yawRad) * cp));
    }
}
