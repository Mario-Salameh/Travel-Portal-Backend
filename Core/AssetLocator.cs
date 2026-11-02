public static class AssetLocator
{
    private static readonly string AssetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Core");

    public static string Image(string imageName)
        => Path.Combine(AssetsPath, "Images", imageName);

    public static string Font(string fontName)
        => Path.Combine(AssetsPath, "Fonts", fontName);
}