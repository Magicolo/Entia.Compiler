using Nodes.Clauses;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Statements;
using Nodes;

public readonly partial struct Factory
{
    public readonly struct Statements
    {
        public readonly Factory Factory;
        public Statements(Factory factory) { Factory = factory; }

        public Node<Return> Return<T>(Node<T> expression) where T : IExpression =>
            Factory.Create<Return>(expression);
        public Node<Block> Block<T>(params Node<T>[] statements) where T : IStatement =>
            Factory.Create<Block, T>(statements);
        public Node<Expression> Expression<T>(in Node<T> expression) where T : IExpression =>
            Factory.Create<Expression>(expression);
        public Node<Declaration> Declaration<T>(in Node<T> declaration) where T : IDeclaration =>
            Factory.Create<Declaration>(declaration);
        public Node<If> If<TCondition, TBody>(in Node<TCondition> condition, in Node<TBody> statement, in Node<Else> @else)
            where TCondition : IExpression where TBody : IStatement =>
            Factory.Create<If>(condition, statement, @else);
        public Node<If> If<TCondition, TBody>(in Node<TCondition> condition, in Node<TBody> statement)
            where TCondition : IExpression where TBody : IStatement =>
            Factory.Create<If>(condition, statement);
    }

    public Statements Statement => new Statements(this);
}