using System.Linq;
using Entia;
using Entia.Modules;
using Entia.Modules.Family;
using Grammars;
using Nodes;

public static class Grammar
{
    public static Entity CSharp(World world)
    {
        var entities = world.Entities();
        var components = world.Components();
        var families = world.Families();
        var factory = world.Factory();

        Node<T> Create<T>(in T grammar) where T : IGrammar
        {
            var child = entities.Create();
            components.Set(child, grammar);
            return new Node<T>(child, grammar, families, components);
        }

        Node<T> Wrap<T>(in T grammar, params Entity[] children) where T : IGrammar
        {
            var node = Create(grammar);
            families.Adopt(node, children);
            return node;
        }

        Entity Reference(Entity reference) => Create(new Reference { Value = reference });
        Entity Any(params Entity[] children) => Wrap(new Any(), children);
        Entity All(params Entity[] children) => Wrap(new All(), children);
        Entity Success() => All();
        // Entity Failure() => Any();
        Entity Postfix<T>(double precedence, Associativity associativity, params Entity[] right) where T : struct, INode =>
            Spawn<T>(Wrap(new Postfix { Precedence = precedence, Associativity = associativity }, All(right)));
        Entity Option(params Entity[] children) => Any(All(children), Success());

        Entity Loop(params Entity[] children)
        {
            var loop = Create(new Any());
            families.Adopt(loop, All(All(children), Reference(loop)));
            families.Adopt(loop, Success());
            return loop;
        }

        Entity Character(char character) => Create(new Character { Value = character });

        Entity Range(char minimum, char maximum)
        {
            var children = new Entity[maximum - minimum + 1];
            for (var i = 0; i < children.Length; i++) children[i] = Character((char)(minimum + i));
            return Any(children);
        }

        Entity String(string @string) => All(@string.Select(Character).ToArray());

        Node<Spawn> Spawn<T>(params Entity[] children) where T : struct, INode =>
            Wrap(new Spawn { Type = typeof(T) }, All(children));

        var trivia = Create(new Any());
        var literal = Create(new Any());
        var identifier = Create(new Any());
        var prefix = Create(new Any());
        var postfix = Create(new Any());
        var expression = Wrap(new Precedence(), prefix, postfix);
        var specification = new
        {
            Trivia = new
            {
                Space = Spawn<Nodes.Trivia.Space>(Any(Character(' '), Character('\t'))),
                Line = Spawn<Nodes.Trivia.Line>(Any(Character('\n'), Character('\r')))
            },
            Literal = new
            {
                Boolean = Spawn<Nodes.Literals.Boolean>(Any(String("true"), String("false"))),
                Null = Spawn<Nodes.Literals.Null>(String("null")),
                Number = Spawn<Nodes.Literals.Number>(
                    Range('0', '9'),
                    Loop(Range('0', '9')),
                    Option(All(Character('.'), Range('0', '9'), Loop(Range('0', '9')))))
            },
            Identifier = new
            {
                Name = Spawn<Nodes.Identifiers.Name>(
                    Any(Character('_'), Character('@'), Range('a', 'z'), Range('A', 'Z')),
                    Loop(Any(Character('_'), Range('a', 'z'), Range('A', 'Z'), Range('0', '9')))),
                Global = Spawn<Nodes.Identifiers.Global>(String("global"))
            },
            Expression = new
            {
                Unary = new
                {
                    Minus = Spawn<Nodes.Expressions.Unary.Minus>(Character('-'), Reference(expression)),
                    Plus = Spawn<Nodes.Expressions.Unary.Plus>(Character('+'), Reference(expression)),
                    Not = Spawn<Nodes.Expressions.Unary.Not>(Character('~'), Reference(expression)),
                },
                Binary = new
                {
                    Add = Postfix<Nodes.Expressions.Binary.Add>(70.0, Associativity.Left,
                        Character('+'), Reference(expression)),
                    Subtract = Postfix<Nodes.Expressions.Binary.Subtract>(70.0, Associativity.Left,
                        Character('-'), Reference(expression)),
                    Multiply = Postfix<Nodes.Expressions.Binary.Multiply>(75.0, Associativity.Left,
                        Character('*'), Reference(expression)),
                    Divide = Postfix<Nodes.Expressions.Binary.Divide>(75.0, Associativity.Left,
                        Character('/'), Reference(expression)),

                    Equal = Postfix<Nodes.Expressions.Binary.Equal>(55.0, Associativity.Right,
                        String("=="), Reference(expression)),
                    NotEqual = Postfix<Nodes.Expressions.Binary.NotEqual>(55.0, Associativity.Right,
                        String("!="), Reference(expression)),
                    Greater = Postfix<Nodes.Expressions.Binary.Greater>(60.0, Associativity.Right,
                        Character('>'), Reference(expression)),
                    GreaterEqual = Postfix<Nodes.Expressions.Binary.GreaterEqual>(60.0, Associativity.Right,
                        String(">="), Reference(expression)),
                    Lesser = Postfix<Nodes.Expressions.Binary.Lesser>(60.0, Associativity.Right,
                        Character('<'), Reference(expression)),
                    LesserEqual = Postfix<Nodes.Expressions.Binary.LesserEqual>(60.0, Associativity.Right,
                        String("<="), Reference(expression)),
                    Assign = Postfix<Nodes.Expressions.Binary.Assign>(15.0, Associativity.Right,
                        Character('='), Reference(expression)),
                    Access = Postfix<Nodes.Expressions.Binary.Access>(120.0, Associativity.Left,
                        Character('.'), Reference(expression)),
                },
                Invocation = Postfix<Nodes.Expressions.Invocation>(120.0, Associativity.Left,
                    Character('('), Option(Reference(expression), Loop(Character(','), Reference(expression))), Character(')')),
                Parenthesized = Spawn<Nodes.Expressions.Parenthesized>(Character('('), Reference(expression), Character(')')),
                Identifier = Spawn<Nodes.Expressions.Identifier>(Reference(identifier)),
                Literal = Spawn<Nodes.Expressions.Literal>(Reference(literal))
            }
        };
        families.Adopt(trivia, specification.Trivia.Space, specification.Trivia.Line);
        families.Adopt(literal, specification.Literal.Boolean, specification.Literal.Null, specification.Literal.Number);
        families.Adopt(identifier, specification.Identifier.Name, specification.Identifier.Global);
        families.Adopt(prefix,
            specification.Expression.Unary.Plus,
            specification.Expression.Unary.Minus,
            specification.Expression.Unary.Not,
            specification.Expression.Parenthesized,
            specification.Expression.Identifier,
            specification.Expression.Literal
        );
        families.Adopt(postfix,
            specification.Expression.Invocation,
            specification.Expression.Binary.Access,
            specification.Expression.Binary.Multiply,
            specification.Expression.Binary.Divide,
            specification.Expression.Binary.Add,
            specification.Expression.Binary.Subtract,
            specification.Expression.Binary.Greater,
            specification.Expression.Binary.GreaterEqual,
            specification.Expression.Binary.Lesser,
            specification.Expression.Binary.LesserEqual,
            specification.Expression.Binary.Equal,
            specification.Expression.Binary.NotEqual,
            specification.Expression.Binary.Assign
        );

        var root = Loop(Any(expression));
        foreach (var descendant in families.Descendants(root, From.Bottom))
        {
            // BUG: this will allow for spaces in front of any character, including between digits of a number
            // if (factory.TryNode<Character>(descendant, out var character))
            //     families.Replace(character, All(Loop(Reference(trivia)), Reference(character)));

            // BUG: cannot 'Replace/Destroy' nodes since they may be referred to;
            // could be fixed by retrieving all references an modifying only nodes that are not referred to
            // if (factory.TryNode<All>(descendant, out var all))
            // {
            //     var children = all.Children().ToArray();
            //     if (children.Length == 1) families.Replace(all, children[0]);
            //     else
            //     {
            //         families.Reject(children);
            //         foreach (var child in children)
            //         {
            //             if (factory.TryNode<All>(child, out var nested))
            //                 families.Adopt(all, nested.Children().ToArray());
            //             else
            //                 families.Adopt(all, child);
            //         }
            //     }
            // }

            // if (factory.TryNode<Any>(descendant, out var any))
            // {
            //     var children = any.Children().ToArray();
            //     if (children.Length == 1) families.Replace(any, children[0]);
            //     else
            //     {
            //         families.Reject(children);
            //         foreach (var child in children)
            //         {
            //             if (factory.TryNode<Any>(child, out var nested))
            //                 families.Adopt(any, nested.Children().ToArray());
            //             else
            //                 families.Adopt(any, child);
            //         }
            //     }
            // }
        }
        return root;
    }
}