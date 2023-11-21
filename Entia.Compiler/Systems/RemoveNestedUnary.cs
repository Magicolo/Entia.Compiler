using Entia;
using Entia.Core;
using Entia.Experimental;
using Entia.Injectables;
using Messages;

public static partial class Systems
{
    public static Node RemoveNestedUnary<T>() where T : struct, Nodes.Expressions.Unary.IUnary =>
        Node.Inject((Factory factory, AllFamilies families, AllEntities entities) =>
        Node.System<OnRun>.RunEach((Entity entity, ref T unary) =>
        {
            var node = factory.Node(entity, unary);
            if (node.Expression().Bind(expression => expression.TryAs<T>()).TryValue(out var child) &&
                child.Expression().TryValue(out var nested))
            {
                families.Replace(node, nested);
                entities.Destroy(node);
            }
        }));
}