using XmlNode = X39.Solutions.Papercraft.Xml.XmlNode;

namespace X39.Solutions.Papercraft.Abstraction;

/// <summary>
/// Represents a continuation clause consumed by a chain-aware transformer.
/// </summary>
/// <param name="Name">The name of the continuation transformer clause.</param>
/// <param name="RemainingLine">The remaining line of the continuation clause.</param>
/// <param name="Nodes">The nodes contained in the continuation clause.</param>
public sealed record TransformerChainClause(
    string Name,
    string RemainingLine,
    IReadOnlyCollection<XmlNode> Nodes);
