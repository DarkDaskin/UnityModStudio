namespace UnityModStudio.Common.Tests;

[TestClass]
public sealed class GameInformationTests
{
    [DataTestMethod]
    [DataRow("", false, "<unknown>")]
    [DataRow("^&#*(", false, "<unknown>")]
    [DataRow("net", false, "<unknown>")]
    [DataRow("netstandard", false, "<unknown>")]
    [DataRow("net20", false, ".NET 2.0")]
    [DataRow("net20", true, ".NET 2.0 Subset")]
    [DataRow("net46", false, ".NET 4.6")]
    [DataRow("net472", false, ".NET 4.7.2")]
    [DataRow("netstandard2.0", false, ".NET Standard 2.0")]
    [DataRow("netstandard2.1", false, ".NET Standard 2.1")]
    public void GetMonoProfileStringTest(string targetFrameworkMoniker, bool isSubsetProfile, string expectedResult)
    {
        var gameInformation = new GameInformation { TargetFrameworkMoniker = targetFrameworkMoniker, IsSubsetProfile = isSubsetProfile };

        var result = gameInformation.GetMonoProfileString();

        Assert.AreEqual(expectedResult, result);
    }
}