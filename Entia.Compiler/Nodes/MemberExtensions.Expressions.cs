using System.Collections.Generic;
using System.Linq;
using Nodes;
using Nodes.Expressions;
using Nodes.Expressions.Binary;
using Nodes.Expressions.Unary;
using Nodes.Identifiers;
using Nodes.Literals;
using Entia.Core;
using Entia.Modules.Family;

public static partial class MemberExtensions
{
    public static Option<Node<IExpression>> Left<T>(in this Node<T> node) where T : IBinary => node.Child<IExpression>();
    public static Option<Node<IExpression>> Right<T>(in this Node<T> node) where T : IBinary => node.Children<IExpression>().Skip(1).FirstOrNone();
    public static Option<Node<IExpression>> Expression<T>(in this Node<T> node) where T : IUnary => node.Child<IExpression>();
    public static Option<Node<Identifier>> Left(in this Node<Assign> node) => node.Child<Identifier>();
    public static Option<Node<IExpression>> Expression(in this Node<Parenthesized> node) => node.Child<IExpression>();
    public static Option<Node<IExpression>> Expression(in this Node<Invocation> node) => node.Child<IExpression>();
    public static IEnumerable<Node<Argument>> Arguments(in this Node<Invocation> node) => node.Children<Argument>();
    public static Option<Node<ILiteral>> Value(in this Node<Literal> node) => node.Child<ILiteral>();
    public static Option<Node<T>> Value<T>(in this Node<Literal> node) where T : ILiteral => node.Child<T>();
    public static Option<Node<IIdentifier>> Value(in this Node<Identifier> node) => node.Child<IIdentifier>();
    public static Option<Node<T>> Value<T>(in this Node<Identifier> node) where T : IIdentifier => node.Child<T>();
}