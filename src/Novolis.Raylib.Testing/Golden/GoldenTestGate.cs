namespace Novolis.Raylib.Testing.Golden;

/// <summary>Opt-in environment gates for golden tests in consumer repos.</summary>
public static class GoldenTestGate
{
    /// <summary>True when the named environment variable is <c>1</c> or <c>true</c>.</summary>
    /// <param name="environmentVariableName">Variable to read.</param>
    /// <returns>Whether the opt-in gate is enabled.</returns>
    public static bool IsOptInEnabled(string environmentVariableName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentVariableName);
        var value = Environment.GetEnvironmentVariable(environmentVariableName)?.Trim();
        return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }
}
