using System.Text;
using System.Xml;
using X39.Solutions.Papercraft.Xml;

namespace X39.Solutions.Papercraft;

internal static class LoweredXmlWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(false);

    public static async ValueTask WriteAsync(
        XmlNodeInformation rootNode,
        Stream outputStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(rootNode);
        ArgumentNullException.ThrowIfNull(outputStream);
        cancellationToken.ThrowIfCancellationRequested();

        var settings = new XmlWriterSettings
        {
            Async = true,
            Encoding = Utf8WithoutBom,
            Indent = true,
            CloseOutput = false,
        };

        await using var writer = XmlWriter.Create(outputStream, settings);
        await writer.WriteStartDocumentAsync()
            .ConfigureAwait(false);
        await WriteNodeAsync(writer, rootNode, cancellationToken)
            .ConfigureAwait(false);
        await writer.WriteEndDocumentAsync()
            .ConfigureAwait(false);
        await writer.FlushAsync()
            .ConfigureAwait(false);
    }

    private static async ValueTask WriteNodeAsync(
        XmlWriter writer,
        XmlNodeInformation node,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await writer.WriteStartElementAsync(null, node.NodeName, node.NodeNamespace)
            .ConfigureAwait(false);

        foreach (var (name, value) in node.Attributes)
        {
            if (IsNamespaceDeclaration(name))
                continue;

            await writer.WriteAttributeStringAsync(null, name, null, value)
                .ConfigureAwait(false);
        }

        if (node.Children.Count is 0)
        {
            if (!string.IsNullOrEmpty(node.TextContent))
            {
                await writer.WriteStringAsync(node.TextContent)
                    .ConfigureAwait(false);
            }
        }
        else
        {
            foreach (var child in node.Children)
            {
                await WriteNodeAsync(writer, child, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        await writer.WriteEndElementAsync()
            .ConfigureAwait(false);
    }

    private static bool IsNamespaceDeclaration(string name)
        => string.Equals(name, "xmlns", StringComparison.Ordinal)
           || name.StartsWith("xmlns:", StringComparison.Ordinal);
}
