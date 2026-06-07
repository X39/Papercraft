using System.ComponentModel;
using System.Text;
using System.Xml;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Exceptions;
using static System.String;

namespace X39.Solutions.Papercraft.Xml;

/// <summary>
/// Class to read a template from a <see cref="XmlReader"/>.
/// </summary>
public sealed class XmlTemplateReader : IDisposable
{
    private sealed record ParsedTransformerBlock(
        string Name,
        string RemainingLine,
        IReadOnlyCollection<XmlNode> Nodes);

    /// <inheritdoc />
    public void Dispose()
    {
        _disposable?.Dispose();
    }

    private readonly TemplateData                      _templateData;
    private readonly DocumentOptions                   _documentOptions;
    private readonly CultureInfo                       _cultureInfo;
    private readonly IReadOnlyCollection<ITransformer> _transformers;
    private readonly IDisposable?                      _disposable;

    /// <summary>
    /// Creates a new <see cref="XmlTemplateReader"/> with the given <paramref name="templateData"/> and <paramref name="transformers"/>.
    /// </summary>
    /// <param name="documentOptions"></param>
    /// <param name="cultureInfo">The culture info to use.</param>
    /// <param name="templateData">The template data to use.</param>
    /// <param name="transformers">The transformers to use.</param>
    public XmlTemplateReader(
        DocumentOptions documentOptions,
        CultureInfo cultureInfo,
        ITemplateData templateData,
        IReadOnlyCollection<ITransformer> transformers
    )
    {
        if (templateData is not TemplateData data)
        {
            _templateData = new TemplateData();
            _disposable   = _templateData.Scope("root", templateData);
        }
        else
        {
            _templateData = data;
        }

        _documentOptions = documentOptions;
        _cultureInfo     = cultureInfo;
        _transformers    = transformers;
    }

    private readonly Stack<XmlStyleInformation> _styles = new();

    private static (int line, int column) Location(XmlReader xmlReader)
    {
        if (xmlReader is IXmlLineInfo xmlLineInfo)
            return (xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
        return (-1, -1);
    }

    /// <summary>
    /// Reads the template from the given <paramref name="reader"/> and returns the root node.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> that will complete when the template has been read,
    ///     returning the root node of the template.
    /// </returns>
    public async Task<XmlNodeInformation> ReadAsync(XmlReader reader, CancellationToken cancellationToken = default)
    {
        var nodeTree = ReadXmlNode(reader);
        await TransformNodeTreeAsync(nodeTree, cancellationToken)
            .ConfigureAwait(false);
        return HandleNode(nodeTree);
    }

    private async Task TransformNodeTreeAsync(XmlNode nodeTree, CancellationToken cancellationToken = default)
    {
        for (var nodeIndex = 0; nodeIndex < nodeTree.Children.Count; nodeIndex++)
        {
            var node = nodeTree[nodeIndex];
            nodeIndex = await TransformNodeAsync(nodeTree, node, nodeIndex, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task<int> TransformNodeAsync(
        XmlNode nodeTree,
        XmlNode node,
        int nodeIndex,
        CancellationToken cancellationToken
    )
    {
        foreach (var (key, value) in node.Attributes)
        {
            if (!value.StartsWith('@'))
                continue;
            // Evaluate potential expressions
            try
            {
                var result = await _templateData.EvaluateAsync(_cultureInfo, value[1..], cancellationToken)
                    .ConfigureAwait(false);

                node.SetAttribute(key, result?.ToString() ?? string.Empty);
            }
            catch (Exception ex)
            {
                // Enrich the exception with additional information
                throw new TransformationEvaluationFailedException(value, node, ex);
            }
        }

        if (node is { IsTextNode: true, Text: { } text } && (text.Contains('@') || text.Contains('{')))
        {
            #if !DEBUG
            try
            {
            #endif
            nodeIndex = await TransformNodeTreeExpressionCandidateAsync(
                nodeIndex,
                nodeTree,
                node,
                text,
                cancellationToken
            );
            #if !DEBUG
            }
            catch (XmlTemplateReaderException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new UnhandledXmlTemplateTransformationException(ex, node);
            }
            #endif
        }
        else
        {
            await TransformNodeTreeAsync(node, cancellationToken)
                .ConfigureAwait(false);
        }

        return nodeIndex;
    }

    private async Task<int> TransformNodeTreeExpressionCandidateAsync(
        int nodeIndex,
        XmlNode nodeTree,
        XmlNode node,
        string text,
        CancellationToken cancellationToken = default
    )
    {
        var builder = new StringBuilder(text.Length);
        var previousIndex = -1;
        do
        {
            var indexOfExpressionStart = text.IndexOf('@', previousIndex + 1);
            if (indexOfExpressionStart == -1)
                break;
            var previousText = text[(previousIndex is -1 ? 0 : previousIndex)..indexOfExpressionStart];
            previousIndex = indexOfExpressionStart;
            builder.Append(previousText);
            if (!char.IsWhiteSpace(previousText.LastOrDefault())
                && indexOfExpressionStart is not 0)
                continue; // No match as @ must be preceded by whitespace or be at the beginning of the string.
            var endOfName = indexOfExpressionStart + 1;

            // Scan for the end of the name.
            while (text.Length > endOfName
                   && (char.IsLetterOrDigit(text[endOfName])
                       || text[endOfName] == '-'
                       || text[endOfName] == '_'))
                endOfName++;

            var name = text[(indexOfExpressionStart + 1)..endOfName];
            var lookAhead = text.IndexOf('(', endOfName);
            if (lookAhead != -1
                && IsNullOrWhiteSpace(text[endOfName..lookAhead]))
            {
                // We got a function here
                var bracketCount = 1;
                var endOfFunction = lookAhead + 1;
                while (bracketCount > 0 && endOfFunction < text.Length)
                {
                    if (text[endOfFunction] == '(')
                        bracketCount++;
                    else if (text[endOfFunction] == ')')
                        bracketCount--;
                    endOfFunction++;
                }

                if (bracketCount > 0)
                    throw new TransformationFunctionMissingClosingBracketException(text, node, bracketCount);
                object? functionResult = null;
                try
                {
                    functionResult = await _templateData.EvaluateAsync(
                            _cultureInfo,
                            text[(indexOfExpressionStart + 1)..endOfFunction],
                            cancellationToken
                        )
                        .ConfigureAwait(false);
                }
                catch (FunctionNotFoundDuringEvaluationException ex)
                {
                    if (!_documentOptions.IgnoreErrors)
                        throw new TransformationFunctionNotFoundException(
                            text[(indexOfExpressionStart + 1)..endOfFunction],
                            node,
                            ex.FunctionName
                        );
                }
                catch
                {
                    if (!_documentOptions.IgnoreErrors)
                        throw;
                }

                previousIndex = endOfFunction;
                AppendValueToStringBuilder(functionResult, builder);
            }
            else if (_transformers.FirstOrDefault((t) => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) is
                     { } transformer)
            {
                // A transformer name was matched
                var parsedBlock = ExtractTransformerBlock(
                    nodeTree,
                    node,
                    nodeIndex,
                    endOfName,
                    text,
                    name,
                    builder
                );
                IReadOnlyCollection<TransformerChainClause> clauses = transformer is IChainedTransformer chainedTransformer
                    ? ConsumeContinuationClauses(chainedTransformer, nodeTree, nodeIndex)
                    : ArraySegment<TransformerChainClause>.Empty;

                var currentNodeIndex = nodeIndex;
                try
                {
                    var transformedNodes = transformer switch
                    {
                        INestedClauseTransformer nested => nested.TransformAsync(
                            _cultureInfo,
                            _templateData,
                            parsedBlock.RemainingLine,
                            ConsumeNestedClauses(nested, parsedBlock.Nodes),
                            cancellationToken
                        ),
                        IChainedTransformer chained => chained.TransformAsync(
                            _cultureInfo,
                            _templateData,
                            parsedBlock.RemainingLine,
                            parsedBlock.Nodes,
                            clauses,
                            cancellationToken
                        ),
                        _ => transformer.TransformAsync(
                            _cultureInfo,
                            _templateData,
                            parsedBlock.RemainingLine,
                            parsedBlock.Nodes,
                            cancellationToken
                        ),
                    };
                    await foreach (var transformedNode in transformedNodes.ConfigureAwait(false))
                    {
                        var scope = _templateData.PeekScope()
                            .ToDictionary((q) => q.Key, (q) => q.Value);
                        transformedNode.Scope = scope;
                        nodeTree.InsertChild(currentNodeIndex++, transformedNode);
                    }
                }
                catch
                {
                    if (!_documentOptions.IgnoreErrors)
                        throw;
                }

                var distanceToEnd = nodeTree.Children.Count - currentNodeIndex;
                for (; nodeIndex < nodeTree.Children.Count - distanceToEnd; nodeIndex++)
                {
                    var lNode = nodeTree[nodeIndex];
                    using var adjustedScope = _templateData.Scope(
                        string.Concat(lNode.Namespace, "+", lNode.Name),
                        lNode.Scope ?? new Dictionary<string, object?>()
                    );
                    nodeIndex = await TransformNodeAsync(nodeTree, lNode, nodeIndex, cancellationToken)
                        .ConfigureAwait(false);
                }

                nodeIndex--;
                break;
            }
            else if (_templateData.TryGetVariable(name, out var variableValue))
            {
                // We got a variable here
                AppendValueToStringBuilder(variableValue, builder);
                previousIndex = endOfName;
            }
            else
            {
                // No match
            }
        } while (previousIndex < text.Length);

        if (previousIndex < 0)
            return nodeIndex;
        if (previousIndex < text.Length)
            builder.Append(text[previousIndex..]);
        node.SetText(builder.ToString());
        return nodeIndex;
    }

    private static ParsedTransformerBlock ExtractTransformerBlock(
        XmlNode nodeTree,
        XmlNode node,
        int nodeIndex,
        int endOfName,
        string text,
        string name,
        StringBuilder? leadingTextBuilder)
    {
        var bracketIndex = text.IndexOf('{', endOfName);
        if (bracketIndex == -1)
            throw new TransformationMissingOpeningBracketException(text, node);

        var transformerBody = text[(endOfName + 1)..bracketIndex];
        var remainingText = text[(bracketIndex + 1)..];
        if (leadingTextBuilder is not null
            && leadingTextBuilder.Length > 0
            && leadingTextBuilder.ToString()
                .All(char.IsWhiteSpace))
            node.SetText(leadingTextBuilder.ToString());
        else
            nodeTree.RemoveChild(node);
        if (!IsNullOrWhiteSpace(remainingText))
        {
            nodeTree.InsertChild(
                nodeIndex,
                new XmlNode(node.Line, node.Column, remainingText.TrimStart()) { Scope = node.Scope, }
            );
        }

        var nodesOfTransformer = new List<XmlNode>();
        var currentNodeIndex = nodeIndex;
        var curlyBracketCount = 1;
        XmlNode? endNode = null;
        for (; currentNodeIndex < nodeTree.Children.Count; currentNodeIndex++)
        {
            var childNode = nodeTree[currentNodeIndex];
            if (!childNode.IsTextNode)
            {
                nodesOfTransformer.Add(childNode);
                continue;
            }

            var childText = childNode.Text
                            #pragma warning disable CA2201
                            ?? throw new NullReferenceException(
                                $"Failed to parse transformer expression '{text}' at L{childNode.Line}:C{childNode.Column}, text node text is null."
                            );
            #pragma warning restore CA2201

            for (var i = 0; i < childText.Length; i++)
            {
                var c = childText[i];
                switch (c)
                {
                    case '{':
                        curlyBracketCount++;
                        break;
                    case '}':
                        curlyBracketCount--;
                        break;
                }

                if (curlyBracketCount is not 0)
                    continue;
                var leadingText = childText[..i];

                if (!IsNullOrWhiteSpace(leadingText))
                {
                    var tmpNode = new XmlNode(childNode.Line, childNode.Column, leadingText.TrimEnd())
                    {
                        Scope = node.Scope,
                    };
                    nodeTree.InsertChild(currentNodeIndex, tmpNode);
                    nodesOfTransformer.Add(tmpNode);
                    currentNodeIndex++;
                }

                var trailingText = childText[(i + 1)..];
                if (!IsNullOrWhiteSpace(trailingText))
                {
                    var tmpNode = new XmlNode(childNode.Line, childNode.Column, trailingText.TrimStart())
                    {
                        Scope = node.Scope,
                    };
                    nodeTree.InsertChild(currentNodeIndex + 1, tmpNode);
                }

                childNode.SetText("}");

                break;
            }

            if (curlyBracketCount > 0)
                nodesOfTransformer.Add(childNode);
            else
            {
                endNode = childNode;
                break;
            }
        }

        if (curlyBracketCount > 0)
            throw new TransformationMissingClosingBracketException(text, node);

        if (endNode is null)
            throw new TransformationMissingEndNodeBracketException(text, node);

        nodeTree.RemoveChild(endNode);
        foreach (var xmlNode in nodesOfTransformer)
        {
            nodeTree.RemoveChild(xmlNode);
        }

        return new ParsedTransformerBlock(name, transformerBody, nodesOfTransformer.AsReadOnly());
    }

    private static List<TransformerChainClause> ConsumeContinuationClauses(
        IChainedTransformer transformer,
        XmlNode nodeTree,
        int nodeIndex)
    {
        var clauses = new List<TransformerChainClause>();
        while (TryFindContinuationStart(transformer, nodeTree, nodeIndex, out var continuationIndex, out var name, out var endOfName))
        {
            var node = nodeTree[continuationIndex];
            var text = node.Text
                       #pragma warning disable CA2201
                       ?? throw new NullReferenceException(
                           $"Failed to parse transformer expression at L{node.Line}:C{node.Column}, text node text is null."
                       );
            #pragma warning restore CA2201
            var block = ExtractContinuationBlock(nodeTree, node, continuationIndex, endOfName, text, name);
            clauses.Add(new TransformerChainClause(block.Name, block.RemainingLine, block.Nodes));
        }

        return clauses;
    }

    private static List<TransformerChainClause> ConsumeNestedClauses(
        INestedClauseTransformer transformer,
        IReadOnlyCollection<XmlNode> nodes)
    {
        var nodeTree = new XmlNode(-1, -1, string.Empty, string.Empty);
        foreach (var node in nodes)
        {
            nodeTree.AddChild(node);
        }

        var clauses = new List<TransformerChainClause>();
        while (nodeTree.Children.Count > 0)
        {
            var node = nodeTree[0];
            if (!node.IsTextNode)
                throw new ArgumentException("Nested transformer clauses cannot contain direct element content.");

            var text = node.Text ?? string.Empty;
            if (IsNullOrWhiteSpace(text))
            {
                nodeTree.RemoveChild(node);
                continue;
            }

            var indexOfExpressionStart = text.IndexOf('@');
            if (indexOfExpressionStart == -1 || !IsNullOrWhiteSpace(text[..indexOfExpressionStart]))
                throw new ArgumentException("Nested transformer clauses cannot contain direct text content.");

            var endOfName = indexOfExpressionStart + 1;
            while (text.Length > endOfName
                   && (char.IsLetterOrDigit(text[endOfName])
                       || text[endOfName] == '-'
                       || text[endOfName] == '_'))
                endOfName++;

            var name = text[(indexOfExpressionStart + 1)..endOfName];
            if (!transformer.ClauseNames.Any((q) => q.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"Nested transformer clause '{name}' is not supported by '{transformer.Name}'.");

            var block = ExtractContinuationBlock(nodeTree, node, 0, endOfName, text, name);
            clauses.Add(new TransformerChainClause(block.Name, block.RemainingLine, block.Nodes));
        }

        return clauses;
    }

    private static ParsedTransformerBlock ExtractContinuationBlock(
        XmlNode nodeTree,
        XmlNode node,
        int nodeIndex,
        int endOfName,
        string text,
        string name)
    {
        var bracketIndex = text.IndexOf('{', endOfName);
        if (bracketIndex == -1)
            throw new TransformationMissingOpeningBracketException(text, node);

        var transformerBody = text[(endOfName + 1)..bracketIndex];
        var remainingText = text[(bracketIndex + 1)..];
        nodeTree.RemoveChild(node);
        if (!IsNullOrWhiteSpace(remainingText))
        {
            nodeTree.InsertChild(
                nodeIndex,
                new XmlNode(node.Line, node.Column, remainingText.TrimStart()) { Scope = node.Scope, }
            );
        }

        var nodesOfTransformer = new List<XmlNode>();
        var currentNodeIndex = nodeIndex;
        var curlyBracketCount = 1;
        XmlNode? endNode = null;
        for (; currentNodeIndex < nodeTree.Children.Count; currentNodeIndex++)
        {
            var childNode = nodeTree[currentNodeIndex];
            if (!childNode.IsTextNode)
            {
                nodesOfTransformer.Add(childNode);
                continue;
            }

            var childText = childNode.Text
                            #pragma warning disable CA2201
                            ?? throw new NullReferenceException(
                                $"Failed to parse transformer expression '{text}' at L{childNode.Line}:C{childNode.Column}, text node text is null."
                            );
            #pragma warning restore CA2201

            for (var i = 0; i < childText.Length; i++)
            {
                var c = childText[i];
                switch (c)
                {
                    case '{':
                        curlyBracketCount++;
                        break;
                    case '}':
                        curlyBracketCount--;
                        break;
                }

                if (curlyBracketCount is not 0)
                    continue;
                var leadingText = childText[..i];

                if (!IsNullOrWhiteSpace(leadingText))
                {
                    var tmpNode = new XmlNode(childNode.Line, childNode.Column, leadingText.TrimEnd())
                    {
                        Scope = node.Scope,
                    };
                    nodeTree.InsertChild(currentNodeIndex, tmpNode);
                    nodesOfTransformer.Add(tmpNode);
                    currentNodeIndex++;
                }

                var trailingText = childText[(i + 1)..];
                if (!IsNullOrWhiteSpace(trailingText))
                {
                    var tmpNode = new XmlNode(childNode.Line, childNode.Column, trailingText.TrimStart())
                    {
                        Scope = node.Scope,
                    };
                    nodeTree.InsertChild(currentNodeIndex + 1, tmpNode);
                }

                childNode.SetText("}");

                break;
            }

            if (curlyBracketCount > 0)
                nodesOfTransformer.Add(childNode);
            else
            {
                endNode = childNode;
                break;
            }
        }

        if (curlyBracketCount > 0)
            throw new TransformationMissingClosingBracketException(text, node);

        if (endNode is null)
            throw new TransformationMissingEndNodeBracketException(text, node);

        nodeTree.RemoveChild(endNode);
        foreach (var xmlNode in nodesOfTransformer)
        {
            nodeTree.RemoveChild(xmlNode);
        }

        return new ParsedTransformerBlock(name, transformerBody, nodesOfTransformer.AsReadOnly());
    }

    private static bool TryFindContinuationStart(
        IChainedTransformer transformer,
        XmlNode nodeTree,
        int nodeIndex,
        out int continuationIndex,
        out string name,
        out int endOfName)
    {
        for (var i = nodeIndex; i < nodeTree.Children.Count; i++)
        {
            var node = nodeTree[i];
            if (!node.IsTextNode)
                break;
            var text = node.Text ?? string.Empty;
            if (IsNullOrWhiteSpace(text))
                continue;
            var indexOfExpressionStart = text.IndexOf('@');
            if (indexOfExpressionStart == -1 || !IsNullOrWhiteSpace(text[..indexOfExpressionStart]))
                break;

            endOfName = indexOfExpressionStart + 1;
            while (text.Length > endOfName
                   && (char.IsLetterOrDigit(text[endOfName])
                       || text[endOfName] == '-'
                       || text[endOfName] == '_'))
                endOfName++;

            name = text[(indexOfExpressionStart + 1)..endOfName];
            var continuationName = name;
            if (transformer.ContinuationNames.Any((q) => q.Equals(continuationName, StringComparison.OrdinalIgnoreCase)))
            {
                continuationIndex = i;
                return true;
            }

            break;
        }

        continuationIndex = -1;
        name = string.Empty;
        endOfName = -1;
        return false;
    }

    private static void AppendValueToStringBuilder(object? functionResult, StringBuilder builder)
    {
        if (functionResult is null)
            builder.Append(string.Empty);
        else
        {
            var type = functionResult.GetType();
            if (type.IsEquivalentTo(typeof(string)))
                builder.Append((string) functionResult);
            else
            {
                var converter = TypeDescriptor.GetConverter(type);
                if (converter.CanConvertTo(typeof(string)))
                    builder.Append(converter.ConvertTo(functionResult, typeof(string)) as string ?? string.Empty);
                else
                    builder.Append(functionResult.ToString() ?? string.Empty);
            }
        }
    }

    private static XmlNode ReadXmlNode(XmlReader reader)
    {
        reader.MoveToContent();
        if (reader.NodeType is not XmlNodeType.Element)
            throw new XmlTemplateNodeTypeMismatchException(
                Location(reader)
                    .line,
                Location(reader)
                    .column,
                reader.NodeType,
                XmlNodeType.Element,
                $"Expected element at L{Location(reader).line}:C{Location(reader).column}."
            );
        var nodeStack = new Stack<XmlNode>();
        var location = Location(reader);
        var nodeName = reader.Name;
        var nodeNamespace = reader.NamespaceURI;
        var isEmptyElement = reader.IsEmptyElement;
        var node = new XmlNode(
            location.line,
            location.column,
            IsNullOrEmpty(nodeNamespace) ? Constants.ControlsNamespace : nodeNamespace,
            nodeName
        );
        if (nodeStack.Count > 0)
            nodeStack.Peek()
                .AddChild(node);
        if (reader.HasAttributes)
        {
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                node.SetAttribute(reader.Name, reader.Value);
            }
        }

        reader.ReadStartElement();
        if (isEmptyElement)
            return node;
        reader.MoveToContent();
        while (reader.NodeType != XmlNodeType.EndElement)
        {
            if (reader.NodeType is XmlNodeType.Text)
            {
                var (line, column) = Location(reader);
                node.AddChild(new XmlNode(line, column, reader.Value.Trim()));
                reader.Read();
            }
            else
            {
                node.AddChild(ReadXmlNode(reader));
            }

            reader.MoveToContent();
        }

        reader.ReadEndElement();
        reader.MoveToContent();

        return node;
    }

    private XmlNodeInformation HandleNode(XmlNode xmlNode)
    {
        if (!Validators.ControlName.IsValid(xmlNode.Name))
            throw new XmlNodeNameException(xmlNode);
        PushStyle();
        var children = new List<XmlNodeInformation>();
        var styleName = $"{xmlNode.Name.ToLower(CultureInfo.InvariantCulture)}.style";
        var effectiveStyle = GetEffectiveStyle();
        foreach (var nodeChild in xmlNode.Children)
        {
            if (nodeChild.Name.Equals(styleName, StringComparison.OrdinalIgnoreCase))
            {
                ReadStyle(nodeChild);
            }
            else
            {
                var information = HandleNode(nodeChild);
                children.Add(information);
            }
        }

        PopStyle();
        if (xmlNode.Children.All((q) => q.IsTextNode))
        {
            var text = string.Concat(xmlNode.Children.Select((q) => q.Text));
            return new XmlNodeInformation(
                xmlNode.Line,
                xmlNode.Column,
                xmlNode.Name,
                xmlNode.Namespace,
                text,
                effectiveStyle.Of(xmlNode.Name, xmlNode.Namespace, xmlNode.Attributes),
                ArraySegment<XmlNodeInformation>.Empty
            );
        }
        else
        {
            return new XmlNodeInformation(
                xmlNode.Line,
                xmlNode.Column,
                xmlNode.Name,
                xmlNode.Namespace,
                xmlNode.Text ?? string.Empty,
                effectiveStyle.Of(xmlNode.Name, xmlNode.Namespace, xmlNode.Attributes),
                children.AsReadOnly()
            );
        }
    }

    private XmlStyleInformation GetEffectiveStyle()
    {
        Dictionary<(string controlName, string controlNamespace), IReadOnlyDictionary<string, string>> effectiveStyles =
            new();
        foreach (var style in _styles.Reverse())
        {
            foreach (var (key, value) in style.GetAll())
            {
                effectiveStyles[key] = value;
            }
        }

        return new XmlStyleInformation(effectiveStyles);
    }

    private void ReadStyle(XmlNode xmlNode)
    {
        var style = CurrentStyle;
        foreach (var nodeChild in xmlNode.Children)
        {
            if (nodeChild.Children.Count != 0)
                throw new XmlStyleInformationCannotNestException(
                    nodeChild.Line,
                    nodeChild.Column,
                    $"A style node (L{nodeChild.Line}:C{nodeChild.Column}) cannot have children."
                );
            style.Set(nodeChild.Line, nodeChild.Column, nodeChild.Name, nodeChild.Namespace, nodeChild.Attributes);
        }
    }

    private void PushStyle()
        => _styles.Push(new XmlStyleInformation());

    private void PopStyle()
        => _styles.Pop();

    private XmlStyleInformation CurrentStyle => _styles.Peek();
}
