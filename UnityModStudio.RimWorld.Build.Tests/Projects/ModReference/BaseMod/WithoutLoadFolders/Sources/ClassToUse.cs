namespace BaseMod;

public class ClassToUse
{
#if GAME_1_5
    public const string Version = "1.5";
#endif
#if GAME_1_6
    public const string Version = "1.6";
#endif

    public static void UseMe() { }
}