namespace Novolis.Raylib.Testing;

/// <summary>Manual timestep for deterministic test logic without wall-clock drift.</summary>
public sealed class DeterministicFrameClock
{
    private float _time;
    private float _delta = 1f / 60f;

    public float Time => _time;

    public float DeltaSeconds => _delta;

    public void SetDelta(float deltaSeconds) => _delta = Math.Max(0f, deltaSeconds);

    public float Step(int frames = 1)
    {
        for (var i = 0; i < frames; i++)
            _time += _delta;
        return _time;
    }

    public void Reset() => _time = 0f;
}
