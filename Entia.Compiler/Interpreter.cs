using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nodes;
using Nodes.Clauses;
using Nodes.Declarations;
using Nodes.Expressions;
using Nodes.Expressions.Binary;
using Nodes.Expressions.Unary;
using Nodes.Identifiers;
using Nodes.Literals;
using Nodes.Statements;
using Entia;
using Entia.Core;
using Entia.Modules;

public static class Interpreter
{
    public delegate Result<object> Invoke(params object[] arguments);

    public sealed class Symbol
    {
        public string Name;
        public object Value;
        public Scope Scope;

        public override string ToString() => $"{{ {Name}: {Value} }}";
    }

    public sealed class Scope : IEnumerable<Symbol>
    {
        public Scope Root => Parent is Scope scope ? scope.Root : this;
        public int Count => _symbols.Count;
        public Symbol this[string name] => _nameToSymbols[name];
        public Symbol this[int index] => _symbols[index];

        public Scope Parent;

        readonly List<Symbol> _symbols = new List<Symbol>();
        readonly Dictionary<string, Symbol> _nameToSymbols = new Dictionary<string, Symbol>();

        public IEnumerable<Scope> Ancestors()
        {
            var scope = Parent;
            while (scope is Scope)
            {
                yield return scope;
                scope = scope.Parent;
            }
        }

        public Symbol Declare(string name, Scope scope = null, object value = null)
        {
            var symbol = new Symbol { Name = name, Scope = scope ?? this, Value = value };
            _symbols.Add(symbol);
            _nameToSymbols[symbol.Name] = symbol;
            return symbol;
        }

        public Result<Symbol> Find(string name)
        {
            if (_nameToSymbols.TryGetValue(name, out var symbol)) return symbol;
            if (Parent is Scope scope) return scope.Find(name);
            return Result.Failure($"Failed to find symbol named '{name}'.");
        }
        public bool TryFind(string name, out Symbol symbol) => Find(name).TryValue(out symbol);

        public IEnumerator<Symbol> GetEnumerator() => _symbols.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static Visitor<Scope, object> Create(World world)
    {
        const string global = "<global>";
        var interpreter = new Visitor<Scope, object>(world.Families(), world.Components());

        Result<object> Visit<T>(Option<Node<T>> node, Scope scope) where T : INode =>
            node.AsResult().Bind(value => interpreter.Visit(value, scope));

        static Result<T> Unwrap<T>(object value) => Result.Cast<T>(value is Symbol symbol ? symbol.Value : value);

        void Identifiers()
        {
            interpreter.Add<Global>((node, scope) => scope.Find(global).Box());
            interpreter.Add<Name>((node, scope) => node.Text().AsResult().Bind(text => scope.Find(text)).Box());
        }

        void Clauses()
        {
            interpreter.Add<Else>((node, scope) => Visit(node.Body(), scope));
        }

        void Declarations()
        {
            interpreter.Add<Function>((node, scope) =>
            {
                if (Option.And(node.Name(), node.Body()).TryValue(out var pair))
                {
                    var child = new Scope { Parent = scope };
                    var parameters = node.Parameters()
                        .Select(parameter => interpreter.Visit(parameter, child))
                        .Cast<Symbol>()
                        .ToArray();
                    var invoke = new Invoke(arguments =>
                    {
                        var body = new Scope { Parent = scope };
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var parameter = parameters[i];
                            var value = i < arguments.Length ? arguments[i] : parameter.Value;
                            body.Declare(parameter.Name, value: value);
                        }
                        return interpreter.Visit(pair.Item2, body);
                    });
                    return scope.Declare(pair.Item1, child, invoke);
                }
                return default;
            });
            interpreter.Add<Parameter>((node, scope) => node.Name().Map(name => scope.Declare(
                name,
                // NOTE: the 'new Scope()' will later be used to declare nodes
                // for now, the scope is empty and has no parent such that nothing can be accessed
                new Scope(),
                Visit(node.Default(), scope).OrDefault())));
            interpreter.Add<Variable>((node, scope) => node.Name().Map(name => scope.Declare(
                name,
                new Scope(),
                Visit(node.Initializer(), scope).OrDefault())));
        }

        void Literals()
        {
            static Result<object> Parse<T, TValue>(in Node<T> node, TryFunc<string, TValue> parse) where T : INode =>
                node.Text().TryValue(out var text) && parse(text, out var value) ? Option.From(value).Box() : null;

            interpreter.Add<Number>((node, scope) => Parse<Number, double>(node, double.TryParse));
            interpreter.Add<Nodes.Literals.Null>((node, scope) => null);
            interpreter.Add<Nodes.Literals.String>((node, scope) => node.Text().TryValue(out var text) ? Option.From(text).Box() : null);
            interpreter.Add<Nodes.Literals.Boolean>((node, scope) => Parse<Nodes.Literals.Boolean, bool>(node, bool.TryParse));
        }

        void Statements()
        {
            interpreter.Add<Block>((node, scope) =>
            {
                var child = new Scope { Parent = scope };
                Result<object> result = null;
                foreach (var statement in node.Statements()) result = interpreter.Visit(statement, child);
                return result;
            });
            interpreter.Add<Return>((node, scope) => Visit(node.Expression(), scope).Bind(Unwrap<object>));
            interpreter.Add<Declaration>((node, scope) => Visit(node.Declaration(), scope).Bind(Unwrap<object>));
            interpreter.Add<Expression>((node, scope) => Visit(node.Expression(), scope).Bind(Unwrap<object>));
            interpreter.Add<If>((node, scope) =>
                Visit(node.Condition(), scope).Bind(Unwrap<IConvertible>).Map(value => value.ToBoolean(null)).Or(false) ?
                Visit(node.Body(), scope) : Visit(node.Else(), scope));
        }

        void Expressions()
        {
            Result<object> Unary<T, TValue>(in Node<T> node, Scope scope, Func<TValue, object> operate) where T : IUnary =>
                Visit(node.Expression(), scope).Bind(Unwrap<TValue>).Map(operate);

            Result<object> Binary<T, TLeft, TRight>(in Node<T> node, Scope scope, Func<TLeft, TRight, object> operate) where T : IBinary =>
                Result.And(Visit(node.Left(), scope).Bind(Unwrap<TLeft>), Visit(node.Right(), scope).Bind(Unwrap<TRight>))
                    .Map(pair => operate(pair.Item1, pair.Item2));

            interpreter.Add<Literal>((node, scope) => Visit(node.Value(), scope));
            interpreter.Add<Identifier>((node, scope) => Visit(node.Value(), scope));
            interpreter.Add<Parenthesized>((node, scope) => Visit(node.Expression(), scope));

            interpreter.Add<Not>((node, scope) => Unary<Not, IConvertible>(node, scope, value => ~value.ToInt64(null)));
            interpreter.Add<Plus>((node, scope) => Unary<Plus, IConvertible>(node, scope, value => +value.ToInt64(null)));
            interpreter.Add<Minus>((node, scope) => Unary<Minus, IConvertible>(node, scope, value => -value.ToDouble(null)));

            interpreter.Add<Add>((node, scope) => Binary<Add, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) + right.ToDouble(null)));
            interpreter.Add<Subtract>((node, scope) => Binary<Subtract, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) - right.ToDouble(null)));
            interpreter.Add<Multiply>((node, scope) => Binary<Multiply, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) * right.ToDouble(null)));
            interpreter.Add<Divide>((node, scope) => Binary<Divide, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) / right.ToDouble(null)));
            interpreter.Add<Equal>((node, scope) => Binary<Equal, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToBoolean(null) == right.ToBoolean(null)));
            interpreter.Add<NotEqual>((node, scope) => Binary<NotEqual, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToBoolean(null) != right.ToBoolean(null)));
            interpreter.Add<Greater>((node, scope) => Binary<Greater, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) > right.ToDouble(null)));
            interpreter.Add<GreaterEqual>((node, scope) => Binary<GreaterEqual, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) >= right.ToDouble(null)));
            interpreter.Add<Lesser>((node, scope) => Binary<Lesser, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) < right.ToDouble(null)));
            interpreter.Add<LesserEqual>((node, scope) => Binary<LesserEqual, IConvertible, IConvertible>(node, scope,
                (left, right) => left.ToDouble(null) <= right.ToDouble(null)));
            interpreter.Add<Assign>((node, scope) => Visit(node.Left(), scope)
                .Cast<Symbol>()
                .Bind(symbol => Visit(node.Right(), scope).Map(value => symbol.Value = value)));
            interpreter.Add<Access>((node, scope) =>
                Visit(node.Left(), scope).Cast<Symbol>().Bind(symbol => Visit(node.Right(), symbol.Scope)));
            interpreter.Add<Invocation>((node, scope) => Visit(node.Expression(), scope)
                .Bind(Unwrap<Invoke>)
                .And(node.Arguments().Select(argument => interpreter.Visit(argument, scope)).All())
                .Bind(pair => pair.Item1(pair.Item2)));
        }

        interpreter.Add<Root>((node, scope) => Result.Try(() =>
        {
            var symbol = scope.Declare(global, new Scope { Parent = scope });
            foreach (var declaration in node.Declarations()) interpreter.Visit(declaration, symbol.Scope);
            return Visit(node.Child<IExpression>(), symbol.Scope);
        }).Flatten());
        interpreter.Add<Argument>((node, scope) => Visit(node.Expression(), scope));

        Identifiers();
        Clauses();
        Declarations();
        Literals();
        Statements();
        Expressions();
        return interpreter;
    }
}