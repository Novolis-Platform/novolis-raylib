using Novolis.Raylib.Abstractions;
using Novolis.Raylib.Testing.Golden;

namespace Novolis.Raylib.Golden;

public sealed class QueuedGoldenStoryRendererTests
{
    [Test]
    public async Task BeginFrame_and_OnFrame_drain_queue_and_invoke_scene_before_capture()
    {
        var order = new List<string>();
        var queue = new RecordingQueue(order);
        var inner = new RecordingRenderer(order);
        IGoldenSceneScript script = new RecordingScene(order);

        var renderer = new QueuedGoldenStoryRenderer(
            inner,
            queue.Drain,
            script);

        renderer.BeginFrame("step-a");
        renderer.OnFrame(0.016f, 320, 240);

        await Assert.That(order).IsEqualTo([
            "drain",
            "scene:step-a",
            "drain",
            "drain",
            "onframe",
        ]);
    }

    private sealed class RecordingQueue(List<string> order)
    {
        public void Drain() => order.Add("drain");
    }

    private sealed class RecordingRenderer(List<string> order) : IRaylibFrameRenderer
    {
        public void OnFrame(float deltaSeconds, int screenWidth, int screenHeight) =>
            order.Add("onframe");
    }

    private sealed class RecordingScene(List<string> order) : IGoldenSceneScript
    {
        public void BeginFrame(string frameId) => order.Add($"scene:{frameId}");
    }
}
