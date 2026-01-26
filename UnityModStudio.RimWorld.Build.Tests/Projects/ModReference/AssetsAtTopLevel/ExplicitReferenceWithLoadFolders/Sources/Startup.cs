using BaseMod;
using Verse;

namespace MyMod;

[StaticConstructorOnStartup]
public static class Startup
{
    public const string BaseVersion = ClassToUse.Version;

    static Startup()
    {
        ClassToUse.UseMe();
    }
}