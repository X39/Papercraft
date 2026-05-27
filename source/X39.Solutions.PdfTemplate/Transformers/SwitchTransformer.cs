using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using X39.Solutions.PdfTemplate.Abstraction;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Transformers;

/// <summary>
/// A transformer that includes the first matching case clause.
/// </summary>
public partial class SwitchTransformer : INestedClauseTransformer
{
    [GeneratedRegex(@"\A\s*(?:(?<operator>[><=!]{1,3}|in)\s+)?(?<expression>.+?)\s*\z")]
    private static partial Regex ParseCase();

    /// <inheritdoc />
    public string Name => "switch";

    /// <inheritdoc />
    public IReadOnlyCollection<string> ClauseNames { get; } = new[] { "case", "default" };

    /// <inheritdoc />
    public IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        CancellationToken cancellationToken = default)
    {
        throw new ArgumentException("The switch transformer requires nested case or default clauses.", nameof(nodes));
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<TransformerChainClause> clauses,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var scope = templateData.Scope("switch");
        var switchExpression = remainingLine.Trim();
        if (switchExpression.Length is 0)
            throw new ArgumentException("The switch transformer requires an expression.", nameof(remainingLine));
        var switchValue = await templateData.EvaluateAsync(cultureInfo, switchExpression, cancellationToken)
            .ConfigureAwait(false);
        IReadOnlyCollection<XmlNode>? defaultNodes = null;
        var defaultSeen = false;

        foreach (var clause in clauses)
        {
            if (clause.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                if (defaultSeen)
                    throw new ArgumentException("The switch transformer can only contain one default clause.", nameof(clauses));
                defaultSeen = true;
                defaultNodes = clause.Nodes;
                if (clause.RemainingLine.IsNotNullOrWhiteSpace())
                    throw new ArgumentException("The default clause cannot have arguments.", nameof(clauses));
                continue;
            }

            if (!clause.Name.Equals("case", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("The switch transformer can only contain case and default clauses.", nameof(clauses));
            if (defaultSeen)
                throw new ArgumentException("The default clause must be the last clause in a switch transformer.", nameof(clauses));

            if (!await MatchesCaseAsync(cultureInfo, templateData, switchValue, clause.RemainingLine, cancellationToken)
                    .ConfigureAwait(false))
                continue;

            foreach (var node in clause.Nodes)
                yield return node.DeepCopy();
            yield break;
        }

        if (defaultNodes is null)
            yield break;
        foreach (var node in defaultNodes)
            yield return node.DeepCopy();
    }

    private static async Task<bool> MatchesCaseAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        object? switchValue,
        string remainingLine,
        CancellationToken cancellationToken)
    {
        var match = ParseCase().Match(remainingLine);
        if (!match.Success)
            throw new ArgumentException(
                "Expression could not be parsed, please make sure it is of the form: " +
                "@case [operator] <expression> (supported operators: >, <, >=, <=, ==, !=, ===, !==, in)",
                nameof(remainingLine));

        var @operator = match.Groups["operator"].Success ? match.Groups["operator"].Value : "==";
        var expression = await templateData.EvaluateAsync(cultureInfo, match.Groups["expression"].Value, cancellationToken)
            .ConfigureAwait(false);
        return @operator switch
        {
            ">"   => Compare(switchValue, expression) > 0,
            "<"   => Compare(switchValue, expression) < 0,
            ">="  => Compare(switchValue, expression) >= 0,
            "<="  => Compare(switchValue, expression) <= 0,
            "=="  => Similar(switchValue, expression),
            "!="  => !Similar(switchValue, expression),
            "===" => Same(switchValue, expression),
            "!==" => !Same(switchValue, expression),
            "in"  => Contains(switchValue, expression),
            _ => throw new ArgumentException(
                "Invalid operator, only >, <, >=, <=, ==, !=, ===, !==, in are supported.",
                nameof(remainingLine)),
        };
    }

    private static bool Similar(object? leftExpression, object? rightExpression)
    {
        if (leftExpression is null && rightExpression is null)
            return true;
        if (leftExpression is string leftString && rightExpression is string rightString)
            return string.Equals(leftString, rightString, StringComparison.OrdinalIgnoreCase);
        return Same(leftExpression, rightExpression);
    }

    private static bool Same(object? leftExpression, object? rightExpression)
    {
        return Equals(leftExpression, rightExpression);
    }

    private static bool Contains(object? leftExpression, object? rightExpression)
    {
        return rightExpression switch
        {
            string rightString when leftExpression is string leftString => rightString.Contains(leftString),
            string rightString when leftExpression is char leftChar => rightString.Contains(leftChar),
            IEnumerable enumerable => enumerable.Cast<object?>().Contains(leftExpression),
            _ => throw new ArgumentException("Right expression must be enumerable.", nameof(rightExpression)),
        };
    }

    private static int Compare(object? variableValue, object? value)
    {
        if (variableValue is null && value is null)
            return 0;
        if (variableValue is null)
            return -1;
        if (value is null)
            return 1;
        if (variableValue is IComparable comparable)
            return comparable.CompareTo(value);
        throw new ArgumentException("Variable is not comparable.", nameof(variableValue));
    }
}
