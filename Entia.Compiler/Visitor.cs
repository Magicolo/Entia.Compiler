using System;
using Nodes;
using Entia;
using Entia.Core;
using Entia.Modules;

public sealed class Visitor<TIn, TOut>
{
    readonly Families _families;
    readonly Entia.Modules.Components _components;
    readonly TypeMap<INode, Func<Entity, INode, TIn, Result<TOut>>> _visits = new TypeMap<INode, Func<Entity, INode, TIn, Result<TOut>>>();

    public Visitor(Families families, Entia.Modules.Components components)
    {
        _families = families;
        _components = components;
    }

    public Result<TOut> Visit(Entity entity, in TIn state)
    {
        if (_components.TryGet<INode>(entity, out var node) && TryGet(node.GetType(), out var visit))
            return visit(entity, node, state);

        return Result.Failure($"Expected to find a '{nameof(INode)}' component on entity '{entity}'.");
    }
    public bool TryGet(System.Type type, out Func<Entity, INode, TIn, Result<TOut>> visit) =>
        _visits.TryGet(type, out visit, true, false);
    public bool Add<TNode>(Func<Node<TNode>, TIn, Result<TOut>> visit) where TNode : INode =>
        _visits.Set<TNode>((entity, component, state) => component is TNode casted ?
            visit(new Node<TNode>(entity, casted, _families, _components), state) : default);

    public bool Remove<TNode>() where TNode : struct, INode => _visits.Remove<TNode>();
    public bool Remove(System.Type type) => _visits.Remove(type);
    public bool Clear() => _visits.Clear();
}