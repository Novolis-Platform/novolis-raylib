using Novolis.Raylib.Interact;

namespace Novolis.Raylib.Testing;

/// <summary>Queues synthetic key presses for test frame renderers (consumed manually in tests).</summary>
public sealed class SimulatedInput
{
    private readonly Queue<KeyboardKey> _keys = new();

    /// <summary>Enqueues a key press for later consumption.</summary>
    /// <param name="key">Key to simulate.</param>
    public void Press(KeyboardKey key) => _keys.Enqueue(key);

    /// <summary>Dequeues the next simulated key press, if any.</summary>
    /// <param name="key">Dequeued key when this returns true.</param>
    /// <returns>True when a key was dequeued.</returns>
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

    /// <summary>Removes all queued key presses.</summary>
    public void Clear() => _keys.Clear();
}
