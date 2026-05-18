
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Merger
{
    public static void MergePdfFiles()
    {
        ConsoleHelper.WriteHeader("📄 PDF MERGER 📄");

        // Ask user to choose input mode
        ConsoleHelper.WriteSubHeader("Input Mode");
        Console.WriteLine();
        ConsoleHelper.WriteMenuOption("1", "Merge all PDF files from a folder");
        ConsoleHelper.WriteMenuOption("2", "Select specific PDF files to merge");
        Console.WriteLine();
        ConsoleHelper.WritePrompt("Choose input mode (1 or 2): ");

        var inputMode = Console.ReadLine()?.Trim();
        var filePaths = new List<string>();
        string? inputDirectory = null;

        if (inputMode == "1")
        {
            // Folder mode
            filePaths = GetFilesFromFolder(out inputDirectory);
        }
        else if (inputMode == "2")
        {
            // Individual files mode
            filePaths = GetIndividualFiles();
        }
        else
        {
            ConsoleHelper.WriteError("Invalid option selected.");
            return;
        }

        if (filePaths.Count < 2)
        {
            ConsoleHelper.WriteError("At least 2 PDF files are required to merge.");
            return;
        }

        // Ask for output directory
        ConsoleHelper.WriteSubHeader("Output Location");
        string outputDirectory;

        if (inputDirectory != null)
        {
            ConsoleHelper.WriteInfo($"Input folder: {inputDirectory}");
            ConsoleHelper.WritePrompt("Save merged PDF to directory (press Enter to use input folder): ");
        }
        else
        {
            ConsoleHelper.WritePrompt("Save merged PDF to directory: ");
        }

        var outputInput = Console.ReadLine()?.Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(outputInput))
        {
            if (inputDirectory != null)
            {
                outputDirectory = inputDirectory;
                ConsoleHelper.WriteInfo($"Using input folder: {outputDirectory}");
            }
            else
            {
                ConsoleHelper.WriteWarning("Directory path cannot be empty.");
                return;
            }
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

    private static List<string> GetFilesFromFolder(out string inputDirectory)
    {
        var filePaths = new List<string>();
        inputDirectory = string.Empty;

        ConsoleHelper.WriteSubHeader("Folder Selection");

        while (true)
        {
            ConsoleHelper.WritePrompt("Enter the folder path containing PDF files: ");
            var folderPath = Console.ReadLine()?.Trim().Trim('"');

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

            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly)
                                    .OrderBy(f => f)
                                    .ToList();

            if (pdfFiles.Count == 0)
            {
                ConsoleHelper.WriteWarning("No PDF files found in the specified folder.");
                continue;
            }

            inputDirectory = folderPath;
            ConsoleHelper.WriteSuccess($"Found {pdfFiles.Count} PDF file(s):");
            ConsoleHelper.WriteDivider();

            foreach (var file in pdfFiles)
            {
                ConsoleHelper.WriteProgress(Path.GetFileName(file));
                filePaths.Add(file);
            }

            Console.WriteLine();
            break;
        }

        return filePaths;
    }

    private static List<string> GetIndividualFiles()
    {
        var filePaths = new List<string>();

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

        return filePaths;
    }
}
