using System.Linq;
using Nodes;
using Nodes.Expressions;
using Nodes.Identifiers;
using Nodes.Literals;

public readonly partial struct Factory
{
    public readonly partial struct Expressions
    {
        public readonly Factory Factory;
        public Expressions(Factory factory) { Factory = factory; }

        public Node<Identifier> Identifier(string name) => Identifier(Factory.Identifier.Name(name));
        public Node<Identifier> Identifier<T>(in Node<T> identifier) where T : IIdentifier => Factory.Create<Identifier>(identifier);
        public Node<Literal> Literal(double value) => Literal(Factory.Literal.Number(value));
        public Node<Literal> Literal(string value) => Literal(Factory.Literal.String(value));
        public Node<Literal> Literal<T>(in Node<T> literal) where T : ILiteral => Factory.Create<Literal>(literal);
        public Node<Parenthesized> Parenthesized<T>(in Node<T> expression) where T : IExpression =>
            Factory.Create<Parenthesized>(expression);

        public Node<Invocation> Invocation<T>(in Node<T> expression, params Node<Argument>[] arguments) where T : IExpression =>
            Factory.Create<Invocation>(expression, arguments.Select(argument => argument.Entity));
    }

    public Expressions Expression => new Expressions(this);
}