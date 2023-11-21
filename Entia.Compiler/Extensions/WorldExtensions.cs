using Entia;
using Entia.Modules;

public static class WorldExtensions
{
    public static Factory Factory(this World world) => new Factory(world.Families(), world.Components(), world.Entities());
}