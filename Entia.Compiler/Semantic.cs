public static class Semantic
{
    public interface ISymbol { string Name { get; } }
    public interface IMembers : ISymbol { ISymbol[] Members { get; } }
    public interface ITyped : ISymbol { ISymbol Type { get; } }
    public interface IValued : ISymbol { object Value { get; set; } }

    public sealed class Module : IMembers
    {
        public string Name { get; }
        public ISymbol[] Members { get; }

        public Module(string name, params ISymbol[] members)
        {
            Name = name;
            Members = members;
        }
    }

    public sealed class Function : ITyped, IMembers
    {
        public string Name { get; }
        public ISymbol Type { get; }
        public Parameter[] Parameters { get; }
        public ISymbol[] Members => Parameters;

        public Function(string name, params Parameter[] parameters)
        {
            Name = name;
            Parameters = parameters;
        }
    }

    public sealed class Parameter : ITyped
    {
        public string Name { get; }
        public ISymbol Type { get; }
        public object Default { get; }

        public Parameter(string name, object @default = null)
        {
            Name = name;
            Default = @default;
        }
    }

    public sealed class Variable : IValued, ITyped
    {
        public string Name { get; }
        public ISymbol Type { get; }
        public object Value { get; set; }

        public Variable(string name, object value = null)
        {
            Name = name;
            Value = value;
        }
    }
}