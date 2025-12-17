using System;
using System.Collections.Generic;
$if$ ($targetframeworkversion$ >= 3.5)using System.Linq;
$endif$using System.Text;
using HarmonyLib;
using Verse;

namespace $projectname$;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        var harmony = new Harmony("$ModPackageId$");
        harmony.PatchAll(typeof(Startup).Assembly);
    }
}