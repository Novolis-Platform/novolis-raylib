namespace Novolis.Raylib.Testing;

/// <summary>Manual timestep for deterministic test logic without wall-clock drift.</summary>
public sealed class DeterministicFrameClock
{
    private float _time;
    private float _delta = 1f / 60f;

    /// <summary>Elapsed simulated time in seconds.</summary>
    public float Time => _time;

    /// <summary>Fixed delta time applied by <see cref="Step"/>.</summary>
    public float DeltaSeconds => _delta;

    /// <summary>Sets the per-step delta in seconds (clamped to zero or greater).</summary>
    /// <param name="deltaSeconds">Seconds added on each step.</param>
    public void SetDelta(float deltaSeconds) => _delta = Math.Max(0f, deltaSeconds);

    /// <summary>Advances simulated time by <paramref name="frames"/> steps.</summary>
    /// <param name="frames">Number of steps (default 1).</param>
    /// <returns>Updated <see cref="Time"/>.</returns>
    public float Step(int frames = 1)
    {
        for (var i = 0; i < frames; i++)
            _time += _delta;
        return _time;
    }

    /// <summary>Resets <see cref="Time"/> to zero.</summary>
    public void Reset() => _time = 0f;
}
