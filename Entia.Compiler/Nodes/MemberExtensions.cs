using System.Collections.Generic;
using Nodes;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Identifiers;
using Entia.Core;
using Entia.Modules.Family;

public static partial class MemberExtensions
{
    public static Option<string> Text<T>(in this Node<T> node) where T : INode => node.Get<Components.Text>().Map(text => text.Value);
    public static Option<string> Name<T>(in this Node<T> node) where T : INamed => node.Child<Name>().Bind(name => name.Text());
    public static Option<Node<Type>> Type<T>(in this Node<T> node) where T : ITyped => node.Child<Type>();

    public static IEnumerable<Node<IDeclaration>> Declarations(in this Node<Root> node) => node.Children<IDeclaration>();

    public static Option<Node<IExpression>> Expression(in this Node<Argument> node) => node.Child<IExpression>();
    public static Option<Node<T>> Expression<T>(in this Node<Argument> node) where T : IExpression => node.Child<T>();
    public static Option<Node<IExpression>> Expression(in this Node<Type> node) => node.Child<IExpression>();
    public static Option<Node<T>> Expression<T>(in this Node<Type> node) where T : IExpression => node.Child<T>();

}