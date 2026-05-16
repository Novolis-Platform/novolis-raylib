using Novolis.Raylib.Interact;

namespace Novolis.Raylib.Testing;

/// <summary>Queues synthetic key presses for test frame renderers (consumed manually in tests).</summary>
public sealed class SimulatedInput
{
    private readonly Queue<KeyboardKey> _keys = new();

    public void Press(KeyboardKey key) => _keys.Enqueue(key);

    public bool TryDequeue(out KeyboardKey key)
    {
        if (_keys.Count == 0)
        {
            key = default;
            return false;
        }

        key = _keys.Dequeue();
        return true;
    }

    public void Clear() => _keys.Clear();
}
