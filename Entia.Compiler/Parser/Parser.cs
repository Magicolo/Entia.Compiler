using System.Linq;
using Entia;
using Entia.Core;
using Entia.Modules;
using Grammars;

public static class Parser
{
    public readonly struct State
    {
        public readonly Entity Root;
        public readonly Entity Current;
        public readonly Stream Stream;
        public readonly double Precedence;

        public State(Entity root, Stream stream) : this(root, root, stream, 0.0) { }

        State(Entity root, Entity current, Stream stream, double precedence)
        {
            Root = root;
            Current = current;
            Stream = stream;
            Precedence = precedence;
        }

        public State With(Entity? current = null, Stream? stream = null, double? precedence = null) =>
            new State(Root, current ?? Current, stream ?? Stream, precedence ?? Precedence);
    }

    public readonly struct Stream
    {
        public static string operator -(in Stream left, in Stream right)
        {
            var count = left.Index - right.Index;
            return left.Text.Substring(right.Index, count);
        }

        public readonly int Index;
        public readonly string Text;

        public Stream(string text) : this(0, text) { }

        Stream(int index, string text)
        {
            Index = index;
            Text = text;
        }

        public bool TryNext(out char character, out Stream next)
        {
            if (Index < Text.Length)
            {
                character = Text[Index];
                next = new Stream(Index + 1, Text);
                return true;
            }

            character = default;
            next = default;
            return false;
        }
    }

    public static Visitor<State, State> Create(Entity grammar, World world)
    {
        var entities = world.Entities();
        var components = world.Components();
        var families = world.Families();
        var visitor = new Visitor<State, State>(world.Families(), world.Components());

        visitor.Add<Character>((node, state) =>
        {
            var value = node.Component.Value;
            if (state.Stream.TryNext(out var character, out var stream) && value == character)
                return state.With(stream: stream);
            else
                return Result.Failure($"Expected character '{value}' but found '{character}'.");
        });
        visitor.Add<All>((node, state) =>
        {
            foreach (var child in node.Children<IGrammar>())
            {
                var result = visitor.Visit(child, state);
                if (result.TryValue(out var next)) state = next;
                else return result;
            }
            return state;
        });
        visitor.Add<Any>((node, state) =>
        {
            foreach (var child in node.Children<IGrammar>())
            {
                var result = visitor.Visit(child, state);
                if (result.TryValue(out var next)) return next;
            }
            return Result.Failure($"Expected at least 1 successful children.");
        });
        visitor.Add<Postfix>((node, state) =>
        {
            var precedence = node.Component.Precedence;
            var associativity = node.Component.Associativity;
            if (precedence <= state.Precedence)
                return Result.Failure($"Expected precedence '{precedence}' to be greater than '{state.Precedence}'.");

            if (node.Child<IGrammar>().TryValue(out var right))
            {
                if (associativity == Associativity.Right) precedence -= 0.0001;
                return visitor.Visit(right, state.With(precedence: precedence));
            }

            return Result.Failure($"Expected to find a '{nameof(IGrammar)}' child.");
        });
        visitor.Add<Precedence>((node, state) =>
        {
            var children = node.Children<IGrammar>().ToArray();
            if (children.Length < 2) return Result.Failure("Missing children.");

            var prefix = children[0];
            var postfix = children[1];
            var precedence = state.Precedence;
            var result = visitor.Visit(prefix, state.With(precedence: 100));
            if (result.TryValue(out var current))
            {
                while (true)
                {
                    result = visitor.Visit(postfix, current.With(precedence: precedence));
                    if (result.TryValue(out var next))
                    {
                        var parent = families.Parent(current.Current);
                        families.AdoptAt(0, next.Current, current.Current);
                        families.Adopt(parent, next.Current);
                        current = next;
                    }
                    else return current.With(precedence: state.Precedence);
                }
            }
            return result;
        });

        visitor.Add<Spawn>((node, state) =>
        {
            if (node.Child<IGrammar>().TryValue(out var child))
            {
                var current = entities.Create();
                components.Set(current, node.Component.Type);
                families.Adopt(state.Current, current);

                var result = visitor.Visit(child, state.With(current: current));
                if (result.TryValue(out var next))
                {
                    var difference = next.Stream - state.Stream;
                    components.Set(current, new Components.Text { Value = difference });
                    return next.With(current: current);
                }

                entities.Destroy(current);
                return result;
            }
            return Result.Failure($"Expected to find a '{nameof(IGrammar)}' child.");
        });
        visitor.Add<Reference>((node, state) => visitor.Visit(node.Component.Value, state));
        return visitor;
    }
}