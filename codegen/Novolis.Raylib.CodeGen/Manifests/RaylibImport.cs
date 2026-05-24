namespace Novolis.Raylib.CodeGen;

internal sealed class RaylibImport
{
    public string? Name { get; set; }

    public string? Template { get; set; }

    public string? Description { get; set; }

    public bool? SuppressGcTransition { get; set; }
}
