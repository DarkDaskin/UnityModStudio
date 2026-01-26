using System;
using System.Collections.Generic;
using System.IO;

namespace UnityModStudio.RimWorld.Build;

public class FileNameComparer : EqualityComparer<string>
{
    public override bool Equals(string x, string y) => StringComparer.OrdinalIgnoreCase.Equals(Path.GetFileName(x), Path.GetFileName(y));

    public override int GetHashCode(string obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(Path.GetFileName(obj));
}