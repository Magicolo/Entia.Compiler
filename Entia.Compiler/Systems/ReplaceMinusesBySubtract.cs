using Entia;
using Entia.Core;
using Entia.Experimental;
using Entia.Injectables;
using Messages;

public static partial class Systems
{
    public static Node ReplaceMinusesBySubtract() =>
        Node.Inject((Factory factory, AllFamilies families, AllEntities entities) =>
        Node.System<OnRun>.RunEach((Entity entity, ref Nodes.Expressions.Unary.Minus minus) =>
        {
            var node = factory.Node(entity, minus);
            if (node.Expression().TryValue(out var expression))
            {
                var subtract = factory.Expression.Binary.Subtract(factory.Expression.Literal(0), expression);
                families.Replace(node, subtract);
                entities.Destroy(node);
            }
        }));
}