using Elsa.Expressions;
using Elsa.Expressions.Contracts;
using Elsa.Expressions.Models;
using Elsa.Workflows.Core.Contracts;
using Elsa.Workflows.Core.Expressions;
using Elsa.Workflows.Core.Services;

namespace Elsa.Workflows.Management.Providers;

/// <inheritdoc />
public class DefaultExpressionSyntaxProvider : IExpressionSyntaxProvider
{
    private readonly IIdentityGenerator _identityGenerator;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultExpressionSyntaxProvider"/> class.
    /// </summary>
    public DefaultExpressionSyntaxProvider(IIdentityGenerator identityGenerator)
    {
        _identityGenerator = identityGenerator;
    }

    /// <inheritdoc />
    public ValueTask<IEnumerable<ExpressionSyntaxDescriptor>> GetDescriptorsAsync(CancellationToken cancellationToken = default)
    {
        var literal = CreateLiteralDescriptor();
        var @object = CreateObjectDescriptor();
        var json = CreateJsonDescriptor();
        var @delegate = CreateDelegateDescriptor();

        return ValueTask.FromResult<IEnumerable<ExpressionSyntaxDescriptor>>(new[] { literal, @object, json, @delegate });
    }

    private ExpressionSyntaxDescriptor CreateLiteralDescriptor() => DefaultExpressionSyntaxProvider.CreateDescriptor<LiteralExpression>(
        "Literal",
        CreateLiteralExpression,
        context => new Literal(context.GetExpression<LiteralExpression>().Value),
        expression => expression.Value);

    private ExpressionSyntaxDescriptor CreateObjectDescriptor() => DefaultExpressionSyntaxProvider.CreateDescriptor<ObjectExpression>(
        "Object",
        CreateObjectExpression,
        context => new ObjectLiteral(context.GetExpression<ObjectExpression>().Value),
        expression => expression.Value);
    
    // TODO: this is replaced by the above and exists only for existing workflow definitions. To be removed in a future version.
    [Obsolete]
    private ExpressionSyntaxDescriptor CreateJsonDescriptor() => DefaultExpressionSyntaxProvider.CreateDescriptor<ObjectExpression>(
        "Json",
        CreateObjectExpression,
        context => new ObjectLiteral(context.GetExpression<ObjectExpression>().Value),
        expression => expression.Value);

    private ExpressionSyntaxDescriptor CreateDelegateDescriptor() => DefaultExpressionSyntaxProvider.CreateDescriptor<DelegateExpression>(
        "Delegate",
        CreateObjectExpression,
        context => new DelegateBlockReference(),
        expression => expression.DelegateBlockReference.Delegate?.ToString());

    private static ExpressionSyntaxDescriptor CreateDescriptor<TExpression>(
        string syntax,
        Func<ExpressionConstructorContext, IExpression> constructor,
        Func<BlockReferenceConstructorContext, MemoryBlockReference> createBlockReference,
        Func<TExpression, object?> expressionValue) =>
        new()
        {
            Syntax = syntax,
            Type = typeof(TExpression),
            CreateExpression = constructor,
            CreateBlockReference = createBlockReference,
            CreateSerializableObject = context => new
            {
                Type = syntax,
                Value = expressionValue(context.GetExpression<TExpression>())
            }
        };

    private static IExpression CreateLiteralExpression(ExpressionConstructorContext context)
    {
        return !context.Element.TryGetProperty("value", out var expressionValue) 
            ? new LiteralExpression() 
            : new LiteralExpression(expressionValue.ToString());
    }

    private IExpression CreateObjectExpression(ExpressionConstructorContext context)
    {
        var expressionValue = context.Element.GetProperty("value").ToString();
        return new ObjectExpression(expressionValue);
    }

    private string GenerateId() => _identityGenerator.GenerateId();
}