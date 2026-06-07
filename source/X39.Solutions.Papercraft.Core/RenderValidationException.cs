namespace X39.Solutions.Papercraft;

/// <summary>
/// Thrown when a renderer cannot satisfy a render request.
/// </summary>
public sealed class RenderValidationException : InvalidOperationException
{
    /// <summary>
    /// Creates a validation exception for the supplied result.
    /// </summary>
    /// <param name="validationResult">The failed validation result.</param>
    public RenderValidationException(RenderValidationResult validationResult)
        : base(CreateMessage(validationResult))
    {
        ValidationResult = validationResult;
    }

    /// <summary>
    /// The validation result that caused the exception.
    /// </summary>
    public RenderValidationResult ValidationResult { get; }

    private static string CreateMessage(RenderValidationResult validationResult)
    {
        var first = validationResult.Diagnostics.Count is 0
            ? null
            : validationResult.Diagnostics[0];
        return first is null
            ? "The render request is not supported by the selected renderer."
            : $"The render request is not supported by the selected renderer: {first.Message}";
    }
}
