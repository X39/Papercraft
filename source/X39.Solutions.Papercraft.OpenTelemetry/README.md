# X39.Solutions.Papercraft.OpenTelemetry

OpenTelemetry integration for Papercraft renderer tracing.

## Register

```csharp
using X39.Solutions.Papercraft.OpenTelemetry;

builder.AddPapercraftOpenTelemetry(
    (tracing) =>
    {
        // Add exporters or processors here.
    });
```

The package registers `X39.Solutions.Papercraft` as an OpenTelemetry activity source. Exporters remain application-owned.
