using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Rotator
{
    public static void RotatePdfPages()
    {
        ConsoleHelper.WriteHeader("🔄 PDF PAGE ROTATOR 🔄");

        // Ask user to choose input mode
        ConsoleHelper.WriteSubHeader("Input Mode");
        Console.WriteLine();
        ConsoleHelper.WriteMenuOption("1", "Rotate pages in a single PDF file");
        ConsoleHelper.WriteMenuOption("2", "Rotate all PDF files in a folder");
        Console.WriteLine();
        ConsoleHelper.WritePrompt("Choose input mode (1 or 2): ");

        var inputMode = Console.ReadLine()?.Trim();

        if (inputMode == "1")
        {
            RotateSingleFile();
        }
        else if (inputMode == "2")
        {
            RotateFolder();
        }
        else
        {
            ConsoleHelper.WriteError("Invalid option selected.");
        }
    }

    /// <summary>
    /// Rotates pages in a single PDF file with options for specific page selection.
    /// </summary>
    private static void RotateSingleFile()
    {
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
        var pagesToRotate = GetPagesToRotate(totalPages);
        if (pagesToRotate.Count == 0)
        {
            return;
        }

        // Ask for rotation direction
        int rotationDegrees = GetRotationDegrees(out string rotationDescription);

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

        // Ask whether to delete original files
        bool deleteOriginal = AskDeleteOriginal();

        // Generate output filename (with timestamp postfix if keeping original)
        var originalFileName = Path.GetFileName(sourcePath);
        var outputFileName = GetOutputFileName(originalFileName, deleteOriginal);
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Rotate pages
        ConsoleHelper.WriteSubHeader("Processing");
        ConsoleHelper.WriteInfo("Rotating PDF pages...");
        ConsoleHelper.WriteDivider();

        try
        {
            // If deleting original, use temp file then replace
            if (deleteOriginal)
            {
                var tempPath = Path.Combine(outputDirectory, $"{Guid.NewGuid()}.pdf.tmp");
                int rotatedCount = RotateFile(sourcePath, tempPath, pagesToRotate, rotationDegrees);

                // Delete original and rename temp to original name
                File.Delete(sourcePath);
                File.Move(tempPath, outputPath);

                ConsoleHelper.WriteInfo($"Replaced original file: {originalFileName}");

                ConsoleHelper.WriteResultBox(
                    "✓ ROTATION COMPLETED SUCCESSFULLY!",
                    $"Pages rotated: {rotatedCount}",
                    $"Rotation: {rotationDescription}",
                    $"File: {outputFileName}",
                    $"Location: {outputDirectory}",
                    "Original file: Replaced"
                );
            }
            else
            {
                int rotatedCount = RotateFile(sourcePath, outputPath, pagesToRotate, rotationDegrees);

                ConsoleHelper.WriteResultBox(
                    "✓ ROTATION COMPLETED SUCCESSFULLY!",
                    $"Pages rotated: {rotatedCount}",
                    $"Rotation: {rotationDescription}",
                    $"Source: {originalFileName}",
                    $"Output: {outputFileName}",
                    $"Location: {outputDirectory}",
                    "Original file: Kept"
                );
            }
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error rotating PDF pages: {ex.Message}");
        }
    }

    /// <summary>
    /// Rotates all pages in all PDF files within a folder.
    /// Output files are saved in the same folder with optional timestamp postfix.
    /// </summary>
    private static void RotateFolder()
    {
        ConsoleHelper.WriteSubHeader("Folder Selection");

        string folderPath;
        List<string> pdfFiles;

        while (true)
        {
            ConsoleHelper.WritePrompt("Enter the folder path containing PDF files: ");
            folderPath = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                ConsoleHelper.WriteWarning("Folder path cannot be empty. Please try again.");
                continue;
            }

            if (!Directory.Exists(folderPath))
            {
                ConsoleHelper.WriteError($"Folder not found: {folderPath}");
                continue;
            }

            pdfFiles = [.. Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly)
                                    .OrderBy(f => f)];

            if (pdfFiles.Count == 0)
            {
                ConsoleHelper.WriteWarning("No PDF files found in the specified folder.");
                continue;
            }

            ConsoleHelper.WriteSuccess($"Found {pdfFiles.Count} PDF file(s):");
            ConsoleHelper.WriteDivider();

            foreach (var file in pdfFiles)
            {
                ConsoleHelper.WriteProgress(Path.GetFileName(file));
            }

            Console.WriteLine();
            break;
        }

        // Ask for rotation direction
        int rotationDegrees = GetRotationDegrees(out string rotationDescription);

        // Ask whether to delete original files
        bool deleteOriginal = AskDeleteOriginal();

        // Rotate all files
        ConsoleHelper.WriteSubHeader("Processing");
        ConsoleHelper.WriteInfo($"Rotating all pages in {pdfFiles.Count} PDF file(s)...");
        ConsoleHelper.WriteDivider();

        int successCount = 0;
        int failCount = 0;
        int totalPagesRotated = 0;

        foreach (var sourcePath in pdfFiles)
        {
            var originalFileName = Path.GetFileName(sourcePath);
            var outputFileName = GetOutputFileName(originalFileName, deleteOriginal);
            var outputPath = Path.Combine(folderPath, outputFileName);

            try
            {
                // Rotate all pages in the file
                using var inputDocument = PdfReader.Open(sourcePath, PdfDocumentOpenMode.Import);
                var allPages = Enumerable.Range(1, inputDocument.PageCount).ToList();

                int rotatedCount;

                // If deleting original, use temp file then replace
                if (deleteOriginal)
                {
                    var tempPath = Path.Combine(folderPath, $"{Guid.NewGuid()}.pdf.tmp");
                    rotatedCount = RotateFile(sourcePath, tempPath, allPages, rotationDegrees);

                    // Delete original and rename temp to original name
                    File.Delete(sourcePath);
                    File.Move(tempPath, outputPath);
                }
                else
                {
                    rotatedCount = RotateFile(sourcePath, outputPath, allPages, rotationDegrees);
                }

                totalPagesRotated += rotatedCount;
                successCount++;

                ConsoleHelper.WriteSuccess($"✓ {originalFileName} → {outputFileName} ({rotatedCount} pages)");
            }
            catch (Exception ex)
            {
                failCount++;
                ConsoleHelper.WriteError($"✗ {originalFileName}: {ex.Message}");
            }
        }

        Console.WriteLine();
        ConsoleHelper.WriteResultBox(
            failCount == 0 ? "✓ FOLDER ROTATION COMPLETED SUCCESSFULLY!" : "⚠ FOLDER ROTATION COMPLETED WITH ERRORS",
            $"Files processed: {successCount}/{pdfFiles.Count}",
            $"Total pages rotated: {totalPagesRotated}",
            $"Rotation: {rotationDescription}",
            $"Output location: {folderPath}",
            deleteOriginal ? "Original files: Replaced" : "Original files: Kept"
        );
    }

    /// <summary>
    /// Prompts the user to select which pages to rotate.
    /// </summary>
    /// <param name="totalPages">The total number of pages in the PDF.</param>
    /// <returns>A list of page numbers to rotate, or an empty list if selection is invalid.</returns>
    private static List<int> GetPagesToRotate(int totalPages)
    {
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
                return [];
            }

            pagesToRotate.Sort();
            ConsoleHelper.WriteSuccess($"Selected {pagesToRotate.Count} page(s): {string.Join(", ", pagesToRotate)}");
        }

        return pagesToRotate;
    }

    /// <summary>
    /// Prompts the user to select a rotation direction.
    /// </summary>
    /// <param name="rotationDescription">Output parameter containing a human-readable description of the rotation.</param>
    /// <returns>The rotation in degrees (90, 180, or 270).</returns>
    private static int GetRotationDegrees(out string rotationDescription)
    {
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

        rotationDescription = rotationDegrees switch
        {
            90 => "90° clockwise (right)",
            180 => "180° (upside down)",
            270 => "270° clockwise (left)",
            _ => $"{rotationDegrees}°"
        };

        ConsoleHelper.WriteInfo($"Rotation: {rotationDescription}");
        return rotationDegrees;
    }

    /// <summary>
    /// Rotates specified pages in a PDF file and saves the result to an output path.
    /// </summary>
    /// <param name="sourcePath">The path to the source PDF file.</param>
    /// <param name="outputPath">The path where the rotated PDF will be saved.</param>
    /// <param name="pagesToRotate">A list of 1-based page numbers to rotate.</param>
    /// <param name="rotationDegrees">The rotation angle in degrees (90, 180, or 270).</param>
    /// <returns>The number of pages that were rotated.</returns>
    private static int RotateFile(string sourcePath, string outputPath, List<int> pagesToRotate, int rotationDegrees)
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
                rotatedCount++;
            }
        }

        inputDocument.Save(outputPath);
        return rotatedCount;
    }

    /// <summary>
    /// Prompts the user to choose whether to delete original files after rotation.
    /// </summary>
    /// <returns>True if user wants to delete original files, false to keep them.</returns>
    private static bool AskDeleteOriginal()
    {
        ConsoleHelper.WriteSubHeader("Original File Handling");
        Console.WriteLine();
        ConsoleHelper.WriteMenuOption("1", "Keep original file(s)");
        ConsoleHelper.WriteMenuOption("2", "Delete original file(s) after rotation");
        Console.WriteLine();
        ConsoleHelper.WritePrompt("Choose option (press Enter for default - Keep): ");

        var choice = Console.ReadLine()?.Trim();
        bool deleteOriginal = choice == "2";

        if (deleteOriginal)
        {
            ConsoleHelper.WriteWarning("Original file(s) will be DELETED after rotation.");
        }
        else
        {
            ConsoleHelper.WriteInfo("Original file(s) will be kept.");
        }

        return deleteOriginal;
    }

    /// <summary>
    /// Generates the output filename based on whether original files will be deleted.
    /// If keeping originals, adds a timestamp postfix to avoid name conflicts.
    /// If deleting originals, uses the same filename.
    /// </summary>
    /// <param name="originalFileName">The original filename including extension.</param>
    /// <param name="deleteOriginal">Whether the original file will be deleted.</param>
    /// <returns>The output filename.</returns>
    private static string GetOutputFileName(string originalFileName, bool deleteOriginal)
    {
        if (deleteOriginal)
        {
            // Same filename since original will be deleted
            return originalFileName;
        }
        else
        {
            // Add timestamp postfix to avoid conflicts
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var extension = Path.GetExtension(originalFileName);
            return $"{nameWithoutExtension}_{timestamp}{extension}";
        }
    }
}
