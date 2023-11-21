using Nodes.Statements;
using Entia;
using Entia.Core;
using Entia.Injectables;
using Entia.Experimental;
using Messages;

public static partial class Systems
{
    public static Node ReplaceFunctionExpressionBodyByBlock() =>
        Node.Inject((Factory factory, AllFamilies families, AllEntities entities) =>
        Node.System<OnRun>.RunEach((Entity entity, ref Nodes.Declarations.Function function) =>
        {
            var node = factory.Node(entity, function);
            if (node.Type().TryValue(out var type) &&
                node.Body<Expression>().TryValue(out var body) &&
                body.Expression().TryValue(out var expression))
            {
                var block = factory.Statement.Block(factory.Statement.Return(expression));
                families.Replace(body, block);
                entities.Destroy(body);
            }
        }));
}