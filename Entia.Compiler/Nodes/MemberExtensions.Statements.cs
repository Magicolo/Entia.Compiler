using System.Collections.Generic;
using Nodes.Clauses;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Statements;
using Entia.Core;
using Entia.Modules.Family;
using Nodes;

public static partial class MemberExtensions
{
    public static IEnumerable<Node<IStatement>> Statements(in this Node<Block> node) => node.Children<IStatement>();
    public static Option<Node<IExpression>> Expression(in this Node<Expression> node) => node.Child<IExpression>();
    public static Option<Node<IExpression>> Expression(in this Node<Return> node) => node.Child<IExpression>();
    public static Option<Node<IExpression>> Condition(in this Node<If> node) => node.Child<IExpression>();
    public static Option<Node<IStatement>> Body(in this Node<If> node) => node.Child<IStatement>();
    public static Option<Node<T>> Body<T>(in this Node<If> node) where T : IStatement => node.Child<T>();
    public static Option<Node<Else>> Else(in this Node<If> node) => node.Child<Else>();
    public static Option<Node<IDeclaration>> Declaration(in this Node<Declaration> node) => node.Child<IDeclaration>();
    public static Option<Node<T>> Declaration<T>(in this Node<Declaration> node) where T : IDeclaration => node.Child<T>();
}