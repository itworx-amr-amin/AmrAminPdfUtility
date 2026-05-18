
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Merger
{
    public static void MergePdfFiles()
    {

        Console.WriteLine("=== Welcome to PDF Merger BY Amr Amin ===\n");

        // Ask for the count of files to merge
        int fileCount;
        while (true)
        {
            Console.Write("Enter the number of PDF files to merge: ");
            if (int.TryParse(Console.ReadLine(), out fileCount) && fileCount >= 2)
            {
                break;
            }
            Console.WriteLine("Please enter a valid number (at least 2 files required).\n");
        }

        // Collect file paths
        var filePaths = new List<string>();
        for (int i = 1; i <= fileCount; i++)
        {
            while (true)
            {
                Console.Write($"Enter the path for PDF file {i}: ");
                var path = Console.ReadLine()?.Trim().Trim('"');

                if (string.IsNullOrWhiteSpace(path))
                {
                    Console.WriteLine("Path cannot be empty. Please try again.\n");
                    continue;
                }

                if (!File.Exists(path))
                {
                    Console.WriteLine($"File not found: {path}. Please try again.\n");
                    continue;
                }

                if (!path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("File must be a PDF. Please try again.\n");
                    continue;
                }

                filePaths.Add(path);
                break;
            }
        }

        // Ask for output directory
        string outputDirectory;
        while (true)
        {
            Console.Write("\nEnter the directory path to save the merged PDF: ");
            outputDirectory = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                Console.WriteLine("Directory path cannot be empty. Please try again.");
                continue;
            }

            if (!Directory.Exists(outputDirectory))
            {
                Console.Write("Directory does not exist. Create it? (y/n): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response == "y" || response == "yes")
                {
                    try
                    {
                        Directory.CreateDirectory(outputDirectory);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to create directory: {ex.Message}");
                        continue;
                    }
                }
                continue;
            }
            break;
        }

        // Generate output filename with datetime in milliseconds
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var outputFileName = $"{timestamp}_Merged.pdf";
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Merge PDFs
        Console.WriteLine("\nMerging PDF files...");

        try
        {
            using var outputDocument = new PdfDocument();

            foreach (var filePath in filePaths)
            {
                Console.WriteLine($"  Processing: {Path.GetFileName(filePath)}");
                using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);

                for (int i = 0; i < inputDocument.PageCount; i++)
                {
                    outputDocument.AddPage(inputDocument.Pages[i]);
                }
            }

            outputDocument.Save(outputPath);

            Console.WriteLine($"\n✓ Successfully merged {filePaths.Count} PDF files!");
            Console.WriteLine($"✓ Total pages: {outputDocument.PageCount}");
            Console.WriteLine($"✓ Output saved to: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error merging PDFs: {ex.Message}");
        }

    }
}
