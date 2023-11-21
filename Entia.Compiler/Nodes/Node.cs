using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Entia;
using Entia.Core;
using Entia.Modules.Family;

namespace Nodes
{
    [DebuggerTypeProxy(typeof(Node<>.View))]
    public readonly struct Node<T> where T : INode
    {
        sealed class View
        {
            public Entity Entity => _node.Entity;
            public T Component => _node.Component;
            public Option<Node<INode>> Root => _node.Root<INode>();
            public Option<Node<INode>> Parent => _node.Parent<INode>();
            public Node<INode>[] Children => _node.Children<INode>().ToArray();
            public Node<INode>[] Ancestors => _node.Ancestors<INode>().ToArray();
            public Node<INode>[] Descendants => _node.Descendants<INode>(From.Top).ToArray();

            readonly Node<T> _node;

            public View(Node<T> node) { _node = node; }
        }

        public static implicit operator Entity(in Node<T> node) => node.Entity;

        public readonly Entity Entity;
        public readonly T Component;

        readonly Entia.Modules.Families _families;
        readonly Entia.Modules.Components _components;

        public Node(Entity entity, in T component, Entia.Modules.Families families, Entia.Modules.Components components)
        {
            Entity = entity;
            Component = component;
            _families = families;
            _components = components;
        }

        public bool Is<TComponent>() where TComponent : INode => Component is TComponent;

        public Node<TComponent> As<TComponent>() where TComponent : INode
        {
            if (Component is TComponent component) return new Node<TComponent>(Entity, component, _families, _components);
            return default;
        }

        public Entity Parent() => _families.Parent(Entity);
        public Slice<Entity>.Read Children() => _families.Children(Entity);
        public IEnumerable<Entity> Ancestors() => _families.Ancestors(Entity);
        public IEnumerable<Entity> Descendants(From from) => _families.Descendants(Entity, from);

        public Option<Node<TComponent>> TryAs<TComponent>() where TComponent : INode
        {
            if (Component is TComponent component) return new Node<TComponent>(Entity, component, _families, _components);
            return Option.None();
        }
        public bool TryAs<TComponent>(out Node<TComponent> node) where TComponent : INode =>
            TryAs<TComponent>().TryValue(out node);

        public Option<TComponent> Get<TComponent>() where TComponent : IComponent
        {
            if (_components.TryGet<TComponent>(Entity, out var component)) return component;
            return Option.None();
        }
        public bool TryGet<TComponent>(out TComponent component) where TComponent : IComponent =>
            Get<TComponent>().TryValue(out component);

        public Option<Node<TComponent>> Root<TComponent>() where TComponent : INode =>
            Parent<TComponent>().TryValue(out var parent) ? parent.Root<TComponent>() : TryAs<TComponent>();
        public Option<Node<TComponent>> Parent<TComponent>() where TComponent : INode => Of<TComponent>(Parent());
        public Option<Node<TComponent>> Child<TComponent>() where TComponent : INode =>
            Children<TComponent>().FirstOrNone();
        public IEnumerable<Node<TComponent>> Children<TComponent>() where TComponent : INode =>
            Children().Select(Of<TComponent>).Choose();
        public Option<Node<TComponent>> Ancestor<TComponent>() where TComponent : INode =>
            Ancestors<TComponent>().FirstOrNone();
        public IEnumerable<Node<TComponent>> Ancestors<TComponent>() where TComponent : INode =>
            Ancestors().Select(Of<TComponent>).Choose();
        public IEnumerable<Node<TComponent>> Descendants<TComponent>(From from) where TComponent : INode =>
            Descendants(from).Select(Of<TComponent>).Choose();
        public Option<Node<TComponent>> Descendant<TComponent>(From from) where TComponent : INode =>
            Descendants<TComponent>(from).FirstOrNone();

        Option<Node<TComponent>> Of<TComponent>(Entity entity) where TComponent : INode
        {
            if (_components.TryGet<TComponent>(entity, out var component))
                return new Node<TComponent>(entity, component, _families, _components);
            return Option.None();
        }

        public override string ToString() => $"{Entity}: {Component}";
    }
}