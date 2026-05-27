using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Optional transformer extension for consuming continuation clauses after the primary transformer block.
/// </summary>
public interface IChainedTransformer : ITransformer
{
    /// <summary>
    /// The names of continuation clauses this transformer can consume.
    /// </summary>
    IReadOnlyCollection<string> ContinuationNames { get; }

    /// <summary>
    /// Transforms the given template element and its continuation clauses into a different format.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use.</param>
    /// <param name="templateData">The data storage available to the template.</param>
    /// <param name="remainingLine">The remaining line of the primary transformer.</param>
    /// <param name="nodes">The nodes contained in the primary transformer.</param>
    /// <param name="clauses">The continuation clauses consumed by this transformer.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>The transformed nodes.</returns>
    IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        IReadOnlyCollection<TransformerChainClause> clauses,
        CancellationToken cancellationToken = default);
}
