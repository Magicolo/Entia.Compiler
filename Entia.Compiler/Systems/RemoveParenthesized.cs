using Nodes.Expressions;
using Entia;
using Entia.Core;
using Entia.Injectables;
using Entia.Experimental;
using Messages;

public static partial class Systems
{
    public static Node RemoveParenthesized() =>
        Node.Inject((Factory factory, AllFamilies families, AllEntities entities) =>
        Node.System<OnRun>.RunEach((Entity entity, ref Parenthesized parenthesized) =>
        {
            var node = factory.Node(entity, parenthesized);
            if (node.Expression().TryValue(out var expression))
            {
                families.Replace(node, expression);
                entities.Destroy(node);
            }
        }));
}