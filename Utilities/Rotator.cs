using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Rotator
{
    public static void RotatePdfPages()
    {
        ConsoleHelper.WriteHeader("🔄 PDF PAGE ROTATOR 🔄");

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

        // Ask for pages to rotate
        ConsoleHelper.WriteSubHeader("Page Selection");
        ConsoleHelper.WriteInfo("Enter page numbers separated by comma (e.g., 1,3,5,7)");
        ConsoleHelper.WriteInfo("Or press Enter to rotate ALL pages.");
        ConsoleHelper.WritePrompt("Pages to rotate: ");

        var pageInput = Console.ReadLine()?.Trim();
        List<int> pagesToRotate;

        if (string.IsNullOrWhiteSpace(pageInput))
        {
            // Rotate all pages
            pagesToRotate = Enumerable.Range(1, totalPages).ToList();
            ConsoleHelper.WriteInfo($"All {totalPages} page(s) will be rotated.");
        }
        else
        {
            // Parse specific pages
            pagesToRotate = [];
            var parts = pageInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                if (int.TryParse(part, out int pageNum))
                {
                    if (pageNum >= 1 && pageNum <= totalPages)
                    {
                        if (!pagesToRotate.Contains(pageNum))
                        {
                            pagesToRotate.Add(pageNum);
                        }
                    }
                    else
                    {
                        ConsoleHelper.WriteWarning($"Page {pageNum} is out of range (1-{totalPages}), skipping.");
                    }
                }
                else
                {
                    ConsoleHelper.WriteWarning($"Invalid page number '{part}', skipping.");
                }
            }

            if (pagesToRotate.Count == 0)
            {
                ConsoleHelper.WriteError("No valid pages selected. Cannot proceed.");
                return;
            }

            pagesToRotate.Sort();
            ConsoleHelper.WriteSuccess($"Selected {pagesToRotate.Count} page(s): {string.Join(", ", pagesToRotate)}");
        }

        // Ask for rotation direction
        ConsoleHelper.WriteSubHeader("Rotation Direction");
        Console.WriteLine();
        ConsoleHelper.WriteMenuOption("1", "Rotate 90° clockwise (right)");
        ConsoleHelper.WriteMenuOption("2", "Rotate 180° (upside down)");
        ConsoleHelper.WriteMenuOption("3", "Rotate 270° clockwise (left)");
        Console.WriteLine();
        ConsoleHelper.WritePrompt("Choose rotation (press Enter for default - 90° clockwise): ");

        var rotationChoice = Console.ReadLine()?.Trim();
        int rotationDegrees = rotationChoice switch
        {
            "2" => 180,
            "3" => 270,
            _ => 90  // Default: 90° clockwise
        };

        var rotationDescription = rotationDegrees switch
        {
            90 => "90° clockwise (right)",
            180 => "180° (upside down)",
            270 => "270° clockwise (left)",
            _ => $"{rotationDegrees}°"
        };

        ConsoleHelper.WriteInfo($"Rotation: {rotationDescription}");

        // Ask for output directory
        ConsoleHelper.WriteSubHeader("Output Location");
        var sourceDirectory = Path.GetDirectoryName(sourcePath) ?? string.Empty;
        ConsoleHelper.WriteInfo($"Source folder: {sourceDirectory}");
        ConsoleHelper.WritePrompt("Save rotated PDF to directory (press Enter to use source folder): ");

        var outputInput = Console.ReadLine()?.Trim().Trim('"');
        string outputDirectory;

        if (string.IsNullOrWhiteSpace(outputInput))
        {
            outputDirectory = sourceDirectory;
            ConsoleHelper.WriteInfo($"Using source folder: {outputDirectory}");
        }
        else
        {
            outputDirectory = outputInput;

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
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelper.WriteError($"Failed to create directory: {ex.Message}");
                        return;
                    }
                }
                else
                {
                    ConsoleHelper.WriteError("Cannot proceed without a valid output directory.");
                    return;
                }
            }
        }

        // Generate output filename with datetime in milliseconds
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var outputFileName = $"{timestamp}_Rotated.pdf";
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Rotate pages
        ConsoleHelper.WriteSubHeader("Processing");
        ConsoleHelper.WriteInfo("Rotating PDF pages...");
        ConsoleHelper.WriteDivider();

        try
        {
            using var inputDocument = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Modify);

            int rotatedCount = 0;
            for (int i = 0; i < inputDocument.PageCount; i++)
            {
                int pageNumber = i + 1;
                if (pagesToRotate.Contains(pageNumber))
                {
                    var page = inputDocument.Pages[i];
                    page.Rotate = (page.Rotate + rotationDegrees) % 360;
                    ConsoleHelper.WriteProgress($"Rotated page {pageNumber}");
                    rotatedCount++;
                }
            }

            inputDocument.Save(outputPath);

            ConsoleHelper.WriteResultBox(
                "✓ ROTATION COMPLETED SUCCESSFULLY!",
                $"Pages rotated: {rotatedCount}",
                $"Rotation: {rotationDescription}",
                $"Source: {Path.GetFileName(sourcePath)}",
                $"Output: {outputFileName}",
                $"Location: {outputDirectory}"
            );
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error rotating PDF pages: {ex.Message}");
        }
    }
}
