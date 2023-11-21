using System.Collections.Generic;
using System.Linq;
using Nodes.Expressions;
using Nodes.Statements;
using Entia;
using Entia.Injectables;
using Entia.Modules.Family;
using Nodes;
using Entia.Experimental;
using Messages;

public static partial class Systems
{
    public static Node DeclareVariablesForExpressions() =>
        Node.Inject((Factory factory, AllFamilies families) =>
        {
            IEnumerable<Node<IStatement>> Statements(IEnumerable<Node<IExpression>> expressions)
            {
                var index = 0;
                foreach (var expression in expressions)
                {
                    var name = $"v_{index++}";
                    var identifier = factory.Expression.Identifier(name);
                    families.Replace(expression, identifier);

                    yield return factory.Statement.Declaration(factory.Declaration.Variable(
                        factory.Identifier.Name(name),
                        factory.Type("var"),
                        expression
                    )).As<IStatement>();
                }
            }

            return Node.System<OnRun>.RunEach((Entity entity, ref Block block) =>
            {
                var node = factory.Node(entity, block);
                var statements = Statements(node.Descendants<IExpression>(From.Bottom)).Concat(node.Statements()).ToArray();
                families.Replace(node, factory.Statement.Block(statements));
            });
        });
}