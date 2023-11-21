using System;
using System.Linq;
using Nodes;
using Nodes.Clauses;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Expressions.Binary;
using Nodes.Expressions.Unary;
using Nodes.Identifiers;
using Nodes.Literals;
using Nodes.Statements;
using Entia;
using Entia.Core;
using Entia.Modules;
using Nodes.Trivia;

public static class Generator
{
    public static Visitor<Unit, string> Create(World world)
    {
        var generator = new Visitor<Unit, string>(world.Families(), world.Components());

        void Trivia()
        {
            generator.Add<Space>((node, _) => node.Text().AsResult());
            generator.Add<Line>((node, _) => node.Text().AsResult());
        }

        void Identifiers()
        {
            generator.Add<Global>((node, _) => "global");
            generator.Add<Name>((node, _) => node.Text().AsResult());
        }

        void Clauses()
        {
            generator.Add<Else>((node, _) => node.Body().AsResult()
                .Bind(body => generator.Visit(body, _))
                .Map(body => $"else {body}"));
        }

        void Literals()
        {
            generator.Add<ILiteral>((node, _) => node.Text().AsResult());
        }

        void Declarations()
        {
            generator.Add<Variable>((node, _) => Result.And(
                node.Name().AsResult(),
                node.Type().AsResult().Bind(type => generator.Visit(type, _)))
                .Map(pair =>
                {
                    var assign = node.Initializer().AsResult()
                        .Bind(initializer => generator.Visit(initializer, _))
                        .Map(initializer => $" = {initializer}")
                        .Or("");
                    return $"{pair.Item2} {pair.Item1}{assign}";
                }));
            generator.Add<Function>((node, _) => Result.And(
                node.Name().AsResult(),
                node.Type().AsResult().Bind(type => generator.Visit(type, _)),
                node.Body().AsResult().Bind(body => generator.Visit(body, _)
                    .Map(value => (body: value, expression: body.Is<Expression>()))),
                node.Parameters().Select(parameter => generator.Visit(parameter, _)).All())
                .Map(values =>
                {
                    var arrow = values.Item3.expression ? " => " : " ";
                    return $"{values.Item2} {values.Item1}({string.Join(", ", values.Item4)}){arrow}{values.Item3.body}";
                }));
            generator.Add<Parameter>((node, _) => Result.And(
                node.Name().AsResult(),
                node.Type().AsResult().Bind(type => generator.Visit(type, _)))
                .Map(pair => $"{pair.Item2} {pair.Item1}"));
        }

        void Statements()
        {
            generator.Add<Declaration>((node, _) => node.Declaration().AsResult()
                .Bind(declaration => generator.Visit(declaration, _))
                .Map(declaration => $"{declaration};"));
            generator.Add<Block>((node, _) => node.Statements()
                .Select(statement => generator.Visit(statement, _))
                .All()
                .Map(statements => $"{{ {string.Join(" ", statements)} }}"));
            generator.Add<If>((node, _) => Result.And(node.Condition().AsResult(), node.Body().AsResult())
                .Bind(pair => Result.And(generator.Visit(pair.Item1, _), generator.Visit(pair.Item2, _)))
                .Map(pair =>
                {
                    var clause = node.Else().AsResult()
                        .Bind(@else => generator.Visit(@else, _))
                        .Map(@else => $" {@else}")
                        .Or("");
                    return $"if({pair.Item1}) {pair.Item2}{clause}";
                }));
            generator.Add<Expression>((node, _) => node.Expression().AsResult()
                .Bind(expression => generator.Visit(expression, _))
                .Map(expression => $"{expression};"));
            generator.Add<Return>((node, _) => node.Expression().AsResult()
                .Bind(expression => generator.Visit(expression, _))
                .Map(expression => $"return {expression};"));
        }

        void Expressions()
        {
            Result<string> Unary<T>(in Node<T> node, string @operator) where T : IUnary =>
                node.Expression().AsResult()
                    .Bind(expression => generator.Visit(expression, default))
                    .Map(expression => $"{@operator}{expression}");

            Result<string> Binary<T>(Node<T> node, string @operator) where T : IBinary =>
                Result.And(node.Left().AsResult(), node.Right().AsResult())
                    .Bind(pair => Result.And(generator.Visit(pair.Item1, default), generator.Visit(pair.Item2, default)))
                    .Map(pair => node.Parent<Nodes.Statements.IStatement>() || node.Parent<Nodes.Declarations.IDeclaration>() ?
                        $"{pair.Item1} {@operator} {pair.Item2}" :
                        $"({pair.Item1} {@operator} {pair.Item2})");

            generator.Add<Identifier>((node, _) => node.Value().AsResult().Bind(value => generator.Visit(value, _)));
            generator.Add<Literal>((node, _) => node.Value().AsResult().Bind(value => generator.Visit(value, _)));
            generator.Add<Invocation>((node, _) => node.Expression().AsResult()
                .Bind(expression => Result.And(
                    generator.Visit(expression, _),
                    node.Arguments().Select(argument => generator.Visit(argument, _)).All()))
                .Map(pair => $"{pair.Item1}({string.Join(", ", pair.Item2)})"));
            generator.Add<Parenthesized>((node, _) => node.Expression().AsResult()
                .Bind(expression => generator.Visit(expression, _))
                .Map(expression => $"({expression})"));

            generator.Add<Minus>((node, _) => Unary(node, "-"));
            generator.Add<Plus>((node, _) => Unary(node, "+"));
            generator.Add<Not>((node, _) => Unary(node, "~"));

            generator.Add<Add>((node, _) => Binary(node, "+"));
            generator.Add<Subtract>((node, _) => Binary(node, "-"));
            generator.Add<Divide>((node, _) => Binary(node, "/"));
            generator.Add<Multiply>((node, _) => Binary(node, "*"));
            generator.Add<Equal>((node, _) => Binary(node, "=="));
            generator.Add<NotEqual>((node, _) => Binary(node, "!="));
            generator.Add<Greater>((node, _) => Binary(node, ">"));
            generator.Add<GreaterEqual>((node, _) => Binary(node, ">="));
            generator.Add<Lesser>((node, _) => Binary(node, "<"));
            generator.Add<LesserEqual>((node, _) => Binary(node, "<="));
            generator.Add<Assign>((node, _) => Binary(node, "="));
            generator.Add<Access>((node, _) => Result.And(node.Left().AsResult(), node.Right().AsResult())
                .Bind(pair => Result.And(
                    generator.Visit(pair.Item1, _)
                        .Map(value => (value, global:
                            pair.Item1.TryAs<Identifier>(out var expression) &&
                            expression.Value().TryValue(out var identifier) &&
                            identifier.Is<Global>())),
                    generator.Visit(pair.Item2, _)))
                .Map(pair => $"{pair.Item1}{(pair.Item2 ? "::" : ".")}{pair.Item3}"));
        }

        generator.Add<Root>((node, _) => node.Children()
            .Select(child => generator.Visit(child, _))
            .All()
            .Map(values => string.Join(Environment.NewLine, values)));
        generator.Add<Argument>((node, _) => node.Expression().AsResult().Bind(expression => generator.Visit(expression, _)));

        Trivia();
        Identifiers();
        Clauses();
        Declarations();
        Literals();
        Statements();
        Expressions();
        return generator;
    }
}