namespace Novolis.Raylib.Testing;

/// <summary>Documents env gates for TUnit tests (set NOVOLIS_RAYLIB_NATIVE_TESTS=1 to run).</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RunOnlyIfNativeRaylibAttribute : Attribute
{
}
