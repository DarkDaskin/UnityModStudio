namespace UnityModStudio.Common.Tests;

[TestClass]
public sealed class GameVersionComparerTests
{
    [DataTestMethod]
    [DataRow(null, "1", -1)]
    [DataRow("1", null, 1)]
    [DataRow(null, null, 0)]
    public void Null_CompareAsLesser(string? x, string? y, int expectedResult) => ExecuteCommonTest(x, y, expectedResult);

    [DataTestMethod]
    [DataRow("1.0", "1.0", 0)]
    [DataRow("1.0", "2.0", -1)]
    [DataRow("2.1", "2.0", 1)]
    [DataRow("2.1", "2.11", -1)]
    [DataRow("2.1", "2.1.1", -1)]
    [DataRow("2.1", "2.2.1", -1)]
    public void NumericVersion_ComparePartsNumerically(string x, string y, int expectedResult) => ExecuteCommonTest(x, y, expectedResult);

    [DataTestMethod]
    [DataRow("1.0-pre", "1.0-pre", 0)]
    [DataRow("1.0-pre.1", "1.0-pre.11", -1)]
    [DataRow("1.0", "1.0-pre", 1)]
    [DataRow("1.1", "1.0-pre", 1)]
    [DataRow("1.0-alpha", "1.0-beta", -1)]
    [DataRow("1.1-alpha", "1.0-beta", 1)]
    public void PreReleaseVersion_ComparePartsNumericallyWhenPossible(string x, string y, int expectedResult) => ExecuteCommonTest(x, y, expectedResult);

    [DataTestMethod]
    [DataRow("^*(%^)", "^*(%^)", 0)]
    [DataRow("^=(%^)", "^*(%^)", 1)]
    public void MalformedVersion_CompareLexically(string x, string y, int expectedResult) => ExecuteCommonTest(x, y, expectedResult);

    private static void ExecuteCommonTest(string? x, string? y, int expectedResult)
    {
        var comparer = new GameVersionComparer();

        var result = comparer.Compare(x, y);

        // The IComparer<> contract allows any return values, only sign is important.
        switch (expectedResult)
        {
            case <0:
                Assert.IsTrue(result < 0, "Expected value less than 0, got {0} instead.", result);
                break;
            case >0:
                Assert.IsTrue(result > 0, "Expected value greater than 0, got {0} instead.", result);
                break;
            default:
                Assert.AreEqual(expectedResult, result);
                break;
        }        
    }
}
