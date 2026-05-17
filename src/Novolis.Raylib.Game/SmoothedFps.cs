namespace Novolis.Raylib.Game;

/// <summary>Exponential moving average of frames per second.</summary>
public sealed class SmoothedFps
{
    private float _value;
    private bool _initialized;

    public float Alpha { get; set; } = 0.1f;

    public float Value => _value;

    public void Update(float deltaSeconds)
    {
        if (deltaSeconds <= 1e-6f)
            return;

        var instant = 1f / deltaSeconds;
        _value = _initialized ? _value * (1f - Alpha) + instant * Alpha : instant;
        _initialized = true;
    }
}
