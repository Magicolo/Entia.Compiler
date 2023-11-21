using Entia;
using Entia.Experimental;
using Entia.Injectables;
using Entia.Queryables;
using Entia.Systems;

// namespace Systems
// {
//     // TODO: define optimizers for the parser
//     public struct MergeNestedAll : IRunEach<Grammars.All>
//     {
//         [All(typeof(Grammars.All))]
//         public Group<Entity> Group;
//         public AllFamilies Families;
//         public Factory Factory;


//         // TODO: All(All(...)) can be flattened to All(...)
//         public void Run(Entity entity, ref Grammars.All component1)
//         {
//         }
//     }

//     // TODO: when Range.Minimum == Range.Maximum, convert to Character
//     public struct ConvertSingleRangeToCharacter : IRun
//     {
//         public void Run()
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }