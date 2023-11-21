using Nodes.Clauses;
using Nodes.Statements;
using Nodes;

public readonly partial struct Factory
{
    public readonly struct Clauses
    {
        public readonly Factory Factory;
        public Clauses(Factory factory) { Factory = factory; }

        public Node<Else> Else<T>(in Node<T> body) where T : IStatement => Factory.Create<Else>(body);
    }

    public Clauses Clause => new Clauses(this);
}