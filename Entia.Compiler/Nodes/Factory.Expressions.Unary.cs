using Nodes.Expressions;
using Nodes.Expressions.Unary;
using Nodes;

public readonly partial struct Factory
{
    public readonly partial struct Unaries
    {
        public readonly Factory Factory;
        public Unaries(Factory factory) { Factory = factory; }

        public Node<T> Create<T, TExpression>(in Node<TExpression> expression) where T : struct, IUnary where TExpression : IExpression =>
            Factory.Create<T>(expression);
        public Node<Plus> Plus<T>(in Node<T> expression) where T : IExpression => Create<Plus, T>(expression);
        public Node<Minus> Minus<T>(in Node<T> expression) where T : IExpression => Create<Minus, T>(expression);
        public Node<Not> Not<T>(in Node<T> expression) where T : IExpression => Create<Not, T>(expression);
    }

    public readonly partial struct Expressions
    {
        public Unaries Unary => new Unaries(Factory);
    }
}