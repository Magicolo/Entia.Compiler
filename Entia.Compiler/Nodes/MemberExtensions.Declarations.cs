using System.Collections.Generic;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Statements;
using Entia.Core;
using Entia.Modules.Family;
using Nodes;

public static partial class MemberExtensions
{
    public static Option<Node<IStatement>> Body(in this Node<Function> node) => node.Child<IStatement>();
    public static Option<Node<T>> Body<T>(in this Node<Function> node) where T : IStatement => node.Child<T>();
    public static IEnumerable<Node<Parameter>> Parameters(in this Node<Function> node) => node.Children<Parameter>();

    public static Option<Node<IExpression>> Initializer(in this Node<Variable> node) => node.Child<IExpression>();
    public static Option<Node<T>> Initializer<T>(in this Node<Variable> node) where T : IExpression => node.Child<T>();
    public static Option<Node<IExpression>> Default(in this Node<Parameter> node) => node.Child<IExpression>();
    public static Option<Node<T>> Default<T>(in this Node<Parameter> node) where T : IExpression => node.Child<T>();
}