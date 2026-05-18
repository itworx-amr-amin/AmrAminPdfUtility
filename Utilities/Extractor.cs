using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Extractor
{
    public static void ExtractPdfFiles()
    {
        ConsoleHelper.WriteHeader("✂️ PDF PAGE EXTRACTOR ✂️");

        // Ask for the source PDF file
        ConsoleHelper.WriteSubHeader("Source File");
        string sourcePath;
        int totalPages;
        while (true)
        {
            ConsoleHelper.WritePrompt("Enter the PDF file path: ");
            sourcePath = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                ConsoleHelper.WriteWarning("Path cannot be empty. Please try again.");
                continue;
            }

            if (!File.Exists(sourcePath))
            {
                ConsoleHelper.WriteError($"File not found: {sourcePath}");
                continue;
            }

            if (!sourcePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.WriteWarning("File must be a PDF. Please try again.");
                continue;
            }

            // Get total page count
            try
            {
                using var tempDoc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
                totalPages = tempDoc.PageCount;
                ConsoleHelper.WriteSuccess($"Loaded: {Path.GetFileName(sourcePath)}");
                ConsoleHelper.WriteInfo($"This PDF contains {totalPages} page(s).");
                break;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteError($"Error reading PDF: {ex.Message}");
                continue;
            }
        }

        // Ask for page range
        ConsoleHelper.WriteSubHeader("Page Range");

        // Ask for start page
        int startPage;
        while (true)
        {
            ConsoleHelper.WritePrompt($"Start page (1 to {totalPages}): ");
            if (int.TryParse(Console.ReadLine(), out startPage) && startPage >= 1 && startPage <= totalPages)
            {
                break;
            }
            ConsoleHelper.WriteWarning($"Please enter a valid page number between 1 and {totalPages}.");
        }

        // Ask for end page
        int endPage;
        while (true)
        {
            ConsoleHelper.WritePrompt($"End page ({startPage} to {totalPages}): ");
            if (int.TryParse(Console.ReadLine(), out endPage) && endPage >= startPage && endPage <= totalPages)
            {
                break;
            }
            ConsoleHelper.WriteWarning($"Please enter a valid page number between {startPage} and {totalPages}.");
        }

        ConsoleHelper.WriteInfo($"Will extract pages {startPage} to {endPage} ({endPage - startPage + 1} page(s)).");

        // Ask for output directory
        ConsoleHelper.WriteSubHeader("Output Location");
        string outputDirectory;
        while (true)
        {
            ConsoleHelper.WritePrompt("Save extracted PDF to directory: ");
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
        var outputFileName = $"{timestamp}_Extracted_Pages_{startPage}-{endPage}.pdf";
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Extract pages
        ConsoleHelper.WriteSubHeader("Processing");
        ConsoleHelper.WriteInfo("Extracting PDF pages...");
        ConsoleHelper.WriteDivider();

        try
        {
            using var inputDocument = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            for (int i = startPage - 1; i < endPage; i++)
            {
                ConsoleHelper.WriteProgress($"Extracting page {i + 1}...");
                outputDocument.AddPage(inputDocument.Pages[i]);
            }

            outputDocument.Save(outputPath);

            var extractedCount = endPage - startPage + 1;
            ConsoleHelper.WriteResultBox(
                "✓ EXTRACTION COMPLETED SUCCESSFULLY!",
                $"Pages extracted: {extractedCount}",
                $"Range: Pages {startPage} to {endPage}",
                $"Source: {Path.GetFileName(sourcePath)}",
                $"Output: {outputFileName}",
                $"Location: {outputDirectory}"
            );
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error extracting PDF pages: {ex.Message}");
        }
    }
}