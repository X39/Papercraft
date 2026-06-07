using X39.Solutions.Papercraft.Xml;

namespace X39.Solutions.Papercraft.Exceptions;

/// <summary>
/// Thrown during the transformation of an XML document if a transformer is missing an opening bracket.
/// </summary>
public sealed class TransformationMissingOpeningBracketException : XmlTemplateTransformationException
{
    /// <summary>
    /// The transformer text that failed to parse.
    /// </summary>
    public string TransformerText { get; }
    internal TransformationMissingOpeningBracketException(string text, XmlNode node)
        : base(
            $"Failed to parse transformer expression '{text}' at L{node.Line}:C{node.Column}, missing opening bracket ('{{').",
            node)
    {
        TransformerText = text;
    }
}