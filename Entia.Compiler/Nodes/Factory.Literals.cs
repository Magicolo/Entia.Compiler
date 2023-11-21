using Nodes.Literals;
using Nodes;

public readonly partial struct Factory
{
    public readonly struct Literals
    {
        public readonly Factory Factory;
        public Literals(Factory factory) { Factory = factory; }

        public Node<T> Create<T>(object value) where T : struct, INode => Factory.Create<T>(value.ToString());
        public Node<Boolean> Boolean(bool value) => Create<Boolean>(value);
        public Node<Number> Number(double value) => Create<Number>(value);
        public Node<String> String(string value) => Create<String>(value);
    }

    public Literals Literal => new Literals(this);
}