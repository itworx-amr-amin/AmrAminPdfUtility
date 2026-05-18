
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Merger
{
    public static void MergePdfFiles()
    {
        ConsoleHelper.WriteHeader("📄 PDF MERGER 📄");

        // Ask for the count of files to merge
        int fileCount;
        while (true)
        {
            ConsoleHelper.WritePrompt("How many PDF files would you like to merge? ");
            if (int.TryParse(Console.ReadLine(), out fileCount) && fileCount >= 2)
            {
                ConsoleHelper.WriteInfo($"Great! You'll be merging {fileCount} PDF files.");
                break;
            }
            ConsoleHelper.WriteWarning("Please enter a valid number (at least 2 files required).");
        }

        // Collect file paths
        ConsoleHelper.WriteSubHeader("Input Files");
        var filePaths = new List<string>();
        for (int i = 1; i <= fileCount; i++)
        {
            while (true)
            {
                ConsoleHelper.WritePrompt($"PDF file {i} of {fileCount}: ");
                var path = Console.ReadLine()?.Trim().Trim('"');

                if (string.IsNullOrWhiteSpace(path))
                {
                    ConsoleHelper.WriteWarning("Path cannot be empty. Please try again.");
                    continue;
                }

                if (!File.Exists(path))
                {
                    ConsoleHelper.WriteError($"File not found: {path}");
                    continue;
                }

                if (!path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleHelper.WriteWarning("File must be a PDF. Please try again.");
                    continue;
                }

                ConsoleHelper.WriteSuccess($"Added: {Path.GetFileName(path)}");
                filePaths.Add(path);
                break;
            }
        }

        // Ask for output directory
        ConsoleHelper.WriteSubHeader("Output Location");
        string outputDirectory;
        while (true)
        {
            ConsoleHelper.WritePrompt("Save merged PDF to directory: ");
            outputDirectory = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                ConsoleHelper.WriteWarning("Directory path cannot be empty. Please try again.");
                continue;
            }

            if (!Directory.Exists(outputDirectory))
            {
                ConsoleHelper.WritePrompt("Directory doesn't exist. Create it? (y/n): ");
                var response = Console.ReadLine()?.Trim().ToLower();
                if (response == "y" || response == "yes")
                {
                    try
                    {
                        Directory.CreateDirectory(outputDirectory);
                        ConsoleHelper.WriteSuccess("Directory created successfully!");
                        break;
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteError($"Failed to create directory: {ex.Message}");
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
        ConsoleHelper.WriteSubHeader("Processing");
        ConsoleHelper.WriteInfo("Merging PDF files...");
        ConsoleHelper.WriteDivider();

        try
        {
            using var outputDocument = new PdfDocument();

            foreach (var filePath in filePaths)
            {
                ConsoleHelper.WriteProgress($"Processing: {Path.GetFileName(filePath)}");
                using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);

                for (int i = 0; i < inputDocument.PageCount; i++)
                {
                    outputDocument.AddPage(inputDocument.Pages[i]);
                }
            }

            outputDocument.Save(outputPath);

            ConsoleHelper.WriteResultBox(
                "✓ MERGE COMPLETED SUCCESSFULLY!",
                $"Files merged: {filePaths.Count}",
                $"Total pages: {outputDocument.PageCount}",
                $"Output: {outputFileName}",
                $"Location: {outputDirectory}"
            );
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error merging PDFs: {ex.Message}");
        }
    }
}
