namespace UnityModStudio.Common.Tests;

public abstract class StoreTestsBase
{
    protected static Type ResourceType { get; set; } = typeof(StoreTestsBase);

    protected string StorePath { get; private set; } = null!;

    [TestInitialize]
    public void CommonTestInitialize()
    {
        StorePath = Path.GetTempFileName();
    }

    [TestCleanup]
    public void CommonTestCleanup()
    {
        if (File.Exists(StorePath))
            File.Delete(StorePath);
    }

    protected void SetStoreFile(string fileName)
    {
        using var inputStream = GetResourceStream(fileName);
        using var outputStream = File.OpenWrite(StorePath);
        inputStream.CopyTo(outputStream);
    }

    protected void VerifyStoreEquals(string fileName)
    {
        using var reader1 = new StreamReader(GetResourceStream(fileName));
        using var reader2 = new StreamReader(File.OpenRead(StorePath));
        Assert.AreEqual(reader1.ReadToEnd(), reader2.ReadToEnd());
    }

    protected static Stream GetResourceStream(string fileName) =>
        ResourceType.Assembly.GetManifestResourceStream(ResourceType, $"Resources.{fileName}") ??
        throw new ArgumentException("Resource not found.", nameof(fileName));
}