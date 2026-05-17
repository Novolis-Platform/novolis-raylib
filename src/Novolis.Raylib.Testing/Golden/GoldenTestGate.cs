namespace Novolis.Raylib.Testing.Golden;

/// <summary>Opt-in environment gates for golden tests in consumer repos.</summary>
public static class GoldenTestGate
{
    public static bool IsOptInEnabled(string environmentVariableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentVariableName);
        var value = Environment.GetEnvironmentVariable(environmentVariableName)?.Trim();
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
