using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Optional transformer extension for consuming nested clauses from the primary transformer block.
/// </summary>
public interface INestedClauseTransformer : ITransformer
{
    /// <summary>
    /// The names of nested clauses this transformer can consume.
    /// </summary>
    IReadOnlyCollection<string> ClauseNames { get; }

    /// <summary>
    /// Transforms the given template element and its nested clauses into a different format.
    /// </summary>
    /// <param name="cultureInfo">The culture info to use.</param>
    /// <param name="templateData">The data storage available to the template.</param>
    /// <param name="remainingLine">The remaining line of the primary transformer.</param>
    /// <param name="clauses">The nested clauses consumed by this transformer.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>The transformed nodes.</returns>
    IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<TransformerChainClause> clauses,
        CancellationToken cancellationToken = default);
}
