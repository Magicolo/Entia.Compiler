using System;
using Nodes;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Literals;
using Entia;
using Entia.Modules;
using Entia.Modules.Family;

public static class Syntax
{
    static readonly Random _random = new Random();
    static T Random<T>(params (double weight, Func<T> provider)[] pairs)
    {
        var sum = 0.0;
        for (int i = 0; i < pairs.Length; i++) sum = pairs[i].weight;

        var value = _random.NextDouble() * sum;
        sum = 0.0;
        for (int i = 0; i < pairs.Length; i++)
        {
            var (weight, provider) = pairs[i];
            sum += weight;
            if (value < sum) return provider();
        }

        return default;
    }

    public static Node<Root> Generate(World world)
    {
        var families = world.Families();
        var factory = world.Factory();

        Node<ILiteral> Literal() => Random<Node<ILiteral>>(
            (1, () => factory.Literal.Number(_random.Next(100)).As<ILiteral>()),
            (1, () => factory.Literal.Boolean(_random.NextDouble() < 0.5).As<ILiteral>()),
            (1, () => factory.Literal.String(_random.Next(100).ToString()).As<ILiteral>())
        );

        Node<IExpression> Expression(int depth = 1)
        {
            Node<IExpression> Unary<T>() where T : struct, Nodes.Expressions.Unary.IUnary =>
                factory.Expression.Unary.Create<T, IExpression>(Expression(depth + 1)).As<IExpression>();

            Node<IExpression> Binary<T>() where T : struct, Nodes.Expressions.Binary.IBinary =>
                factory.Expression.Binary.Create<T, IExpression, IExpression>(Expression(depth + 1), Expression(depth + 1)).As<IExpression>();

            return Random<Node<IExpression>>(
                (3.0 / depth, Unary<Nodes.Expressions.Unary.Plus>),
                (3.0 / depth, Unary<Nodes.Expressions.Unary.Minus>),
                (3.0 / depth, Unary<Nodes.Expressions.Unary.Not>),

                (1.0 / depth, Binary<Nodes.Expressions.Binary.Add>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.Subtract>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.Divide>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.Multiply>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.Equal>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.NotEqual>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.Greater>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.GreaterEqual>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.Lesser>),
                (1.0 / depth, Binary<Nodes.Expressions.Binary.LesserEqual>),

                (1.0 / depth, () => factory.Expression.Parenthesized(Expression(depth + 1)).As<IExpression>()),
                (5.0, () => factory.Expression.Literal(Literal()).As<IExpression>())
            );
        }

        Node<Invocation> Invocation(string name) => factory.Expression.Invocation(
            factory.Expression.Identifier(name)
        );

        Node<Function> Function(string name, string @return) => factory.Declaration.Function(
            factory.Identifier.Name(name),
            factory.Type(@return),
            factory.Statement.Expression(Expression()));

        var root = factory.Root(
            Function("Main", "int"),
            Invocation("Main"));
        world.Resolve();
        return root;
    }
}