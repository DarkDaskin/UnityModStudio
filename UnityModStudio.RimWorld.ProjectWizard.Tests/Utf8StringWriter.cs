using System.Text;

namespace UnityModStudio.RimWorld.ProjectWizard.Tests;

public class Utf8StringWriter : StringWriter
{
    public override Encoding Encoding => Encoding.UTF8;
}