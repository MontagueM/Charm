namespace Tomograph;

public static class DirectoryAssert
{
    public static void DirectoryEquals(string expected, string actual)
    {
        // check name
        Assert.AreEqual(SanitisePath(expected), SanitisePath(actual));
    }

    private static string SanitisePath(string path)
    {
        return Path.GetFullPath(path).Replace('\\', '/').Trim('/');
    }
}
