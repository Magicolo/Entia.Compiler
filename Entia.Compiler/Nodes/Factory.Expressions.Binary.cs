using Nodes.Expressions;
using Nodes.Expressions.Binary;
using Nodes;

public readonly partial struct Factory
{
    public readonly partial struct Expressions
    {
        public readonly partial struct Binaries
        {
            public readonly Factory Factory;
            public Binaries(Factory factory) { Factory = factory; }

            public Node<T> Create<T, TLeft, TRight>(in Node<TLeft> left, in Node<TRight> right) where T : struct, IBinary where TLeft : IExpression where TRight : IExpression =>
                Factory.Create<T>(left, right);
            public Node<Add> Add<TLeft, TRight>(in Node<TLeft> left, in Node<TRight> right) where TLeft : IExpression where TRight : IExpression =>
                Create<Add, TLeft, TRight>(left, right);
            public Node<Subtract> Subtract<TLeft, TRight>(in Node<TLeft> left, in Node<TRight> right) where TLeft : IExpression where TRight : IExpression =>
                Create<Subtract, TLeft, TRight>(left, right);
            public Node<Divide> Divide<TLeft, TRight>(in Node<TLeft> left, in Node<TRight> right) where TLeft : IExpression where TRight : IExpression =>
                Create<Divide, TLeft, TRight>(left, right);
            public Node<Multiply> Multiply<TLeft, TRight>(in Node<TLeft> left, in Node<TRight> right) where TLeft : IExpression where TRight : IExpression =>
                Create<Multiply, TLeft, TRight>(left, right);
            public Node<Assign> Assign<T>(in Node<Identifier> left, in Node<T> right) where T : IExpression =>
                Create<Assign, Identifier, T>(left, right);
            public Node<Access> Access<TLeft, TRight>(in Node<TLeft> left, in Node<TRight> right) where TLeft : IExpression where TRight : IExpression =>
                Create<Access, TLeft, TRight>(left, right);
            public Node<Access> Access<TLeft>(in Node<TLeft> left, string right) where TLeft : IExpression =>
                Access(left, Factory.Expression.Identifier(right));
            public Node<Access> Access<TRight>(string left, in Node<TRight> right) where TRight : IExpression =>
                Access(Factory.Expression.Identifier(left), right);
            public Node<Access> Access(string left, string right) =>
                Access(Factory.Expression.Identifier(left), right);
            public Node<Access> Access<TLeft>(in Node<TLeft> left, params string[] accesses) where TLeft : IExpression
            {
                var @this = this;
                Node<IExpression> Next(int index) => index < accesses.Length - 1 ?
                    @this.Access(accesses[index], Next(index + 1)).As<IExpression>() :
                    @this.Factory.Expression.Identifier(accesses[index]).As<IExpression>();
                return Access(left, Next(0));
            }
            public Node<Access> Access(string left, params string[] accesses) =>
                Access(Factory.Expression.Identifier(left), accesses);
        }

        public Binaries Binary => new Binaries(Factory);
    }
}