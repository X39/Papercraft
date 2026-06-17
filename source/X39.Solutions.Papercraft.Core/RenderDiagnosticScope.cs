namespace X39.Solutions.Papercraft;

internal sealed class RenderDiagnosticScope : IDisposable
{
    private static readonly AsyncLocal<RenderDiagnosticScope?> CurrentScope = new();

    private readonly RenderDiagnosticScope? _previous;
    private readonly Action<RenderDiagnostic>? _diagnosticSink;
    private readonly List<RenderDiagnostic> _diagnostics = new();
    private readonly HashSet<RenderDiagnostic> _seen = new();
    private bool _disposed;

    private RenderDiagnosticScope(Action<RenderDiagnostic>? diagnosticSink)
    {
        _previous = CurrentScope.Value;
        _diagnosticSink = diagnosticSink;
        CurrentScope.Value = this;
    }

    public static bool IsActive => CurrentScope.Value is not null;

    public static IReadOnlyList<RenderDiagnostic> CurrentDiagnostics
        => CurrentScope.Value is { } scope
            ? scope._diagnostics
            : Array.Empty<RenderDiagnostic>();

    public IReadOnlyList<RenderDiagnostic> Diagnostics => _diagnostics;

    public static RenderDiagnosticScope Begin(Action<RenderDiagnostic>? diagnosticSink)
        => new(diagnosticSink);

    public static void Report(RenderDiagnostic diagnostic)
    {
        ArgumentNullException.ThrowIfNull(diagnostic);
        CurrentScope.Value?.Add(diagnostic);
    }

    public static void Report(IEnumerable<RenderDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);
        foreach (var diagnostic in diagnostics)
            Report(diagnostic);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        CurrentScope.Value = _previous;
        _disposed = true;
    }

    private void Add(RenderDiagnostic diagnostic)
    {
        if (!_seen.Add(diagnostic))
            return;

        _diagnostics.Add(diagnostic);
        _diagnosticSink?.Invoke(diagnostic);
    }
}
