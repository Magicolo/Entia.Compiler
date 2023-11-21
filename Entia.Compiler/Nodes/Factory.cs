using System.Collections.Generic;
using System.Linq;
using Nodes;
using Nodes.Expressions;
using Entia;
using Entia.Core;
using Entia.Injectables;
using Entia.Injectors;
using Entia.Modules;

public readonly partial struct Factory : IInjectable
{
    [Implementation]
    static readonly Injector<Factory> _injector = Injector.From(context => new Factory(context.World.Families(), context.World.Components(), context.World.Entities()));

    readonly Entia.Modules.Families _families;
    readonly Entia.Modules.Components _components;
    readonly Entia.Modules.Entities _entities;

    public Factory(Entia.Modules.Families families, Entia.Modules.Components components, Entia.Modules.Entities entities)
    {
        _families = families;
        _components = components;
        _entities = entities;
    }

    public Node<T> Create<T, T1, T2>(Node<T1>[] first, params Node<T2>[] second) where T : struct, INode where T1 : INode where T2 : INode =>
        Create<T>(first.Select(node => node.Entity).Concat(second.Select(child => child.Entity)));
    public Node<T> Create<T, TMember>(Entity child1, Entity child2, Entity child3, params Node<TMember>[] children) where T : struct, INode where TMember : INode =>
        Create<T>(children.Select(node => node.Entity).Prepend(child1, child2, child3));
    public Node<T> Create<T, TMember>(Entity child1, Entity child2, params Node<TMember>[] children) where T : struct, INode where TMember : INode =>
        Create<T>(children.Select(node => node.Entity).Prepend(child1, child2));
    public Node<T> Create<T, TMember>(Entity child, params Node<TMember>[] children) where T : struct, INode where TMember : INode =>
        Create<T>(children.Select(node => node.Entity).Prepend(child));
    public Node<T> Create<T, TMember>(params Node<TMember>[] children) where T : struct, INode where TMember : INode =>
        Create<T>(children.Select(node => node.Entity));
    public Node<T> Create<T>(Entity child, IEnumerable<Entity> children) where T : struct, INode =>
        Create<T>(children.Prepend(child));
    public Node<T> Create<T>(Entity child, params Entity[] children) where T : struct, INode =>
        Create<T>(children.Prepend(child));
    public Node<T> Create<T>(IEnumerable<Entity> children) where T : struct, INode =>
        Create<T>(children.ToArray());
    public Node<T> Create<T>(params Entity[] children) where T : struct, INode => Create<T>(null, children);
    public Node<T> Create<T>(string text, params Entity[] children) where T : struct, INode
    {
        var parent = _entities.Create();
        var node = _components.Default<T>();
        _components.Set(parent, node);
        if (text is string) _components.Set(parent, new Components.Text { Value = text });
        for (int i = 0; i < children.Length; i++) _families.Adopt(parent, children[i]);
        return new Node<T>(parent, node, _families, _components);
    }

    public Node<T> Node<T>(Entity entity, T node) where T : INode => new Node<T>(entity, node, _families, _components);
    public bool TryNode<T>(Entity entity, out Node<T> node) where T : INode
    {
        if (_components.TryGet<T>(entity, out var component))
        {
            node = Node(entity, component);
            return true;
        }
        node = default;
        return false;
    }

    public Node<Root> Root(params Entity[] children) => Create<Root>(children);
    public Node<Argument> Argument<T>(in Node<T> expression) where T : IExpression => Create<Argument>(expression);
    public Node<Type> Type<T>(in Node<T> expression) where T : IExpression => Create<Type>(expression);
    public Node<Type> Type(string name) => Create<Type>(Expression.Identifier(name));
    public Node<Type> Type(string name, params string[] names) => Create<Type>(Expression.Binary.Access(name, names));
}