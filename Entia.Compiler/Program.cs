using System;
using Entia;
using Entia.Core;
using Entia.Experimental;
using Entia.Experimental.Scheduling;
using Entia.Modules;
using Messages;

sealed class Program
{
    static readonly Node Compiler = Node.Sequence(
        Systems.RemoveParenthesized,
        // Systems.RemoveNestedUnary<Nodes.Expressions.Unary.Plus>,
        // Systems.RemoveNestedUnary<Nodes.Expressions.Unary.Minus>,
        // Systems.RemoveNestedUnary<Nodes.Expressions.Unary.Not>,
        Systems.ReplaceMinusesBySubtract,
        Systems.ReplaceFunctionExpressionBodyByBlock,
        Systems.DeclareVariablesForExpressions
    );

    static void Main(string[] args)
    {
        var world = new World();
        var messages = world.Messages();
        var controllers = world.Controllers();
        var entities = world.Entities();
        var components = world.Components();
        var families = world.Families();
        var factory = world.Factory();
        var generator = Generator.Create(world);
        var interpreter = Interpreter.Create(world);

        while (true)
        {
            var text = "a=2-3--4";
            var root = factory.Create<Nodes.Root>();
            var grammar = Grammar.CSharp(world);
            var parser = Parser.Create(grammar, world);
            var scope = new Interpreter.Scope();
            scope.Declare("a", value: 6);
            var before = (
                parser.Visit(grammar, new Parser.State(root, new Parser.Stream(text))),
                interpreter.Visit(root, scope),
                generator.Visit(root, default));
            var result = world.Schedule(Compiler).Use(() => messages.Emit<OnRun>());
            var after = (
                interpreter.Visit(root, scope),
                generator.Visit(root, default));
            Console.WriteLine($"{before}");
            Console.WriteLine($"{after}");
            entities.Clear();
        }
    }
}
