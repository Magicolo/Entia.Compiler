using Nodes;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Identifiers;
using Nodes.Statements;
using Entia.Modules.Family;

public readonly partial struct Factory
{
    public readonly struct Declarations
    {
        public readonly Factory Factory;
        public Declarations(Factory factory) { Factory = factory; }

        public Node<Function> Function<T>(in Node<Name> name, in Node<Type> type, in Node<T> body, params Node<Parameter>[] parameters) where T : IStatement =>
            Factory.Create<Function, Parameter>(name, type, body, parameters);
        public Node<Parameter> Parameter<T>(in Node<Name> name, in Node<Type> type, in Node<T> @default) where T : IExpression =>
            Factory.Create<Parameter>(name, type, @default);
        public Node<Parameter> Parameter(in Node<Name> name, in Node<Type> type) =>
            Factory.Create<Parameter>(name, type);
        public Node<Variable> Variable(in Node<Name> name, in Node<Type> type) =>
            Factory.Create<Variable>(name, type);
        public Node<Variable> Variable<T>(in Node<Name> name, in Node<Type> type, in Node<T> initializer) where T : IExpression =>
            Factory.Create<Variable>(name, type, initializer);
    }

    public Declarations Declaration => new Declarations(this);
}