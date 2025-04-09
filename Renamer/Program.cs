namespace Renamer;

internal static class Program
{
    private static void Main(string[] args)
    {
        var rootDirectory = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
        //var rootDirectory = "C:\\Sources\\TheVSAKeeper\\ElementaryGame\\Elementary\\wwwroot\\images";

        var imageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", /*".gif",*/ ".bmp", ".tiff", ".webp", ".avif",
        };

        Console.WriteLine($"Processing directory: {rootDirectory}\n");

        foreach (var filePath in Directory.EnumerateFiles(rootDirectory, "*.*", SearchOption.AllDirectories))
        {
            try
            {
                var extension = Path.GetExtension(filePath);

                if (imageExtensions.Contains(extension))
                {
                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);

                    if (fileNameWithoutExt.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Skipped: {filePath} (already processed)");
                        continue;
                    }

                    var directory = Path.GetDirectoryName(filePath);
                    var newFileName = $"{fileNameWithoutExt}.png{extension}";

                    if (directory == null)
                    {
                        Console.WriteLine($"Skipped: {filePath} (directory is null)");
                        continue;
                    }

                    var newPath = Path.Combine(directory, newFileName);

                    if (File.Exists(newPath))
                    {
                        Console.WriteLine($"Skipped: {filePath} (target exists)");
                    }
                    else
                    {
                        File.Move(filePath, newPath);
                        Console.WriteLine($"Renamed: {Path.GetFileName(filePath)} -> {Path.GetFileName(newPath)}");
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error processing {filePath}: {exception.Message}");
            }
        }

        Console.WriteLine("\nOperation completed!");
    }
}
