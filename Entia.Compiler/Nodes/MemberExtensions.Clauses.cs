using Nodes.Clauses;
using Nodes.Statements;
using Entia.Core;
using Entia.Modules.Family;
using Nodes;

public static partial class MemberExtensions
{
    public static Option<Node<IStatement>> Body(in this Node<Else> node) => node.Child<IStatement>();
}