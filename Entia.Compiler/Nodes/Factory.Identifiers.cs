using Nodes;
using Nodes.Identifiers;

public readonly partial struct Factory
{
    public readonly struct Identifiers
    {
        public readonly Factory Factory;
        public Identifiers(Factory factory) { Factory = factory; }

        public Node<Name> Name(string name) => Factory.Create<Name>(name);
        public Node<Global> Global() => Factory.Create<Global>();
    }

    public Identifiers Identifier => new Identifiers(this);
}