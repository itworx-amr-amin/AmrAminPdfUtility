using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Extractor
{
    public static void ExtractPdfFiles()
    {
        Console.WriteLine("=== Welcome to PDF Page Extractor BY Amr Amin ===\n");

        // Ask for the source PDF file
        string sourcePath;
        int totalPages;
        while (true)
        {
            Console.Write("Enter the path for the source PDF file: ");
            sourcePath = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(sourcePath))
            {
                Console.WriteLine("Path cannot be empty. Please try again.\n");
                continue;
            }

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"File not found: {sourcePath}. Please try again.\n");
                continue;
            }

            if (!sourcePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("File must be a PDF. Please try again.\n");
                continue;
            }

            // Get total page count
            try
            {
                using var tempDoc = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
                totalPages = tempDoc.PageCount;
                Console.WriteLine($"PDF has {totalPages} page(s).\n");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading PDF: {ex.Message}. Please try again.\n");
                continue;
            }
        }

        // Ask for start page
        int startPage;
        while (true)
        {
            Console.Write($"Enter the start page (1 to {totalPages}): ");
            if (int.TryParse(Console.ReadLine(), out startPage) && startPage >= 1 && startPage <= totalPages)
            {
                break;
            }
            Console.WriteLine($"Please enter a valid page number between 1 and {totalPages}.\n");
        }

        // Ask for end page
        int endPage;
        while (true)
        {
            Console.Write($"Enter the end page ({startPage} to {totalPages}): ");
            if (int.TryParse(Console.ReadLine(), out endPage) && endPage >= startPage && endPage <= totalPages)
            {
                break;
            }
            Console.WriteLine($"Please enter a valid page number between {startPage} and {totalPages}.\n");
        }

        // Ask for output directory
        string outputDirectory;
        while (true)
        {
            Console.Write("\nEnter the directory path to save the extracted PDF: ");
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
        var outputFileName = $"{timestamp}_Extracted_Pages_{startPage}-{endPage}.pdf";
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Extract pages
        Console.WriteLine("\nExtracting PDF pages...");

        try
        {
            using var inputDocument = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            for (int i = startPage - 1; i < endPage; i++)
            {
                Console.WriteLine($"  Extracting page: {i + 1}");
                outputDocument.AddPage(inputDocument.Pages[i]);
            }

            outputDocument.Save(outputPath);

            var extractedCount = endPage - startPage + 1;
            Console.WriteLine($"\n✓ Successfully extracted {extractedCount} page(s)!");
            Console.WriteLine($"✓ Pages {startPage} to {endPage} from: {Path.GetFileName(sourcePath)}");
            Console.WriteLine($"✓ Output saved to: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ Error extracting PDF pages: {ex.Message}");
        }
    }
}