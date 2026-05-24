using Novolis.CodeGen.Bindings;

namespace Novolis.Raylib.Manifests;

public sealed class RaylibBindingManifestSource : IBindingManifestSource
{
    public static RaylibBindingManifestSource Instance { get; } = new();

    private RaylibBindingManifestSource() =>
        Fragments =
        [
            RaylibBindingManifests.Raylib6Interop,
            RaylibBindingManifests.ImguiShim,
            RaylibBindingManifests.RayguiShim,
            RaylibBindingManifests.RaylibDebug,
            RaylibBindingManifests.Facades,
            RaylibBindingManifests.Hud,
            RaylibBindingManifests.Gui,
            RaylibBindingManifests.RayguiFacade,
        ];

    public IReadOnlyList<IManifestFragment> Fragments { get; }
}
