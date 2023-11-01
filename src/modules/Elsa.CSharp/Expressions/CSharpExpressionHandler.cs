using Elsa.CSharp.Contracts;
using Elsa.Expressions.Contracts;
using Elsa.Expressions.Models;

namespace Elsa.CSharp.Expressions;

/// <summary>
/// Evaluates c# expressions.
/// </summary>
public class CSharpExpressionHandler : IExpressionHandler
{
    private readonly ICSharpEvaluator _cSharpEvaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpExpressionHandler"/> class.
    /// </summary>
    public CSharpExpressionHandler(ICSharpEvaluator cSharpEvaluator)
    {
        _cSharpEvaluator = cSharpEvaluator;
    }

    /// <inheritdoc />
    public async ValueTask<object?> EvaluateAsync(IExpression expression, Type returnType, ExpressionExecutionContext context)
    {
        var cSharpExpression = (CSharpExpression)expression;
        return await _cSharpEvaluator.EvaluateAsync(cSharpExpression.Value, returnType, context);
    }
}