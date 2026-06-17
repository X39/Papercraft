using Microsoft.Extensions.DependencyInjection;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Primary application-facing Papercraft service.
/// </summary>
public sealed class Papercraft
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Creates a new Papercraft service.
    /// </summary>
    /// <param name="serviceScopeFactory">The service scope factory used to create isolated sessions.</param>
    public Papercraft(IServiceScopeFactory serviceScopeFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <summary>
    /// Creates an isolated Papercraft session with its own template data.
    /// </summary>
    /// <returns>The created session.</returns>
    public PapercraftSession CreateSession()
    {
        var scope = _serviceScopeFactory.CreateAsyncScope();
        try
        {
            return PapercraftSession.Create(scope);
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }
}
