using Entia;
using Nodes;

namespace Grammars
{
    public interface IGrammar : INode { }
    public enum Associativity { Left, Right }

    public struct Character : IGrammar { public char Value; }
    public struct All : IGrammar { }
    public struct Any : IGrammar { }
    public struct Spawn : IGrammar { public System.Type Type; }
    public struct Reference : IGrammar { public Entity Value; }
    public struct Postfix : IGrammar { public double Precedence; public Associativity Associativity; }
    public struct Precedence : IGrammar { }

    // public struct Range : IParser { public char Minimum, Maximum; }
    // public struct String : IParser { public string Value; }
}