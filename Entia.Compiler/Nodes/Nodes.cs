using Entia;

namespace Nodes
{
    public interface INode : IComponent { }
    public interface INamed : INode { }
    public interface ITyped : INode { }
    public struct Root : INode { }
    public struct Argument : INode { }
    public struct Type : INode { }

    namespace Trivia
    {
        public interface ITrivia : INode { }
        public struct Space : ITrivia { }
        public struct Line : ITrivia { }
    }

    namespace Literals
    {
        public interface ILiteral : INode { }
        public struct Null : ILiteral { }
        public struct Number : ILiteral { }
        public struct String : ILiteral { }
        public struct Boolean : ILiteral { }
    }

    namespace Declarations
    {
        public interface IDeclaration : INode { }

        public struct Function : IDeclaration, INamed, ITyped { }
        public struct Variable : IDeclaration, INamed, ITyped { }
        public struct Parameter : IDeclaration, INamed, ITyped { }
    }

    namespace Clauses
    {
        public interface IClause : INode { }

        public struct Else : IClause { }
    }

    namespace Statements
    {
        public interface IStatement : INode { }

        public struct Declaration : IStatement { }
        public struct Expression : IStatement { }
        public struct Block : IStatement { }
        public struct Return : IStatement { }
        public struct If : IStatement { }
    }

    namespace Expressions
    {
        public interface IExpression : INode { }

        namespace Unary
        {
            public interface IUnary : IExpression { }
            public struct Plus : IUnary { }
            public struct Minus : IUnary { }
            public struct Not : IUnary { }
        }

        namespace Binary
        {
            public interface IBinary : IExpression { }
            public struct Add : IBinary { }
            public struct Subtract : IBinary { }
            public struct Multiply : IBinary { }
            public struct Divide : IBinary { }
            public struct Access : IBinary { }
            public struct Assign : IBinary { }
            public struct Equal : IBinary { }
            public struct NotEqual : IBinary { }
            public struct Greater : IBinary { }
            public struct GreaterEqual : IBinary { }
            public struct Lesser : IBinary { }
            public struct LesserEqual : IBinary { }
        }

        public struct Parenthesized : IExpression { }
        public struct Invocation : IExpression { }
        public struct Literal : IExpression { }
        public struct Identifier : IExpression { }
    }

    namespace Identifiers
    {
        public interface IIdentifier : INode { }

        public struct Global : IIdentifier { }
        public struct Name : IIdentifier { }
    }
}