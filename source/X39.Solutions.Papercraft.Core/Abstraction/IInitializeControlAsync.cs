namespace X39.Solutions.Papercraft.Abstraction;

/// <summary>
/// Interface that allows a control to be initialized asynchronously after it and all its children have been created.
/// </summary>
public interface IInitializeControlAsync
{
    /// <summary>
    /// Initializes the control asynchronously.
    /// </summary>
    /// <param name="context">
    /// The consumer-defined context for the current document generation request.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token to cancel the execution.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task InitializeControlAsync(object? context, CancellationToken cancellationToken = default);
}
