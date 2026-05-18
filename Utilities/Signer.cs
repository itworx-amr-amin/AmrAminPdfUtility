using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace AmrAminPdfUtility.Utilities;

public static class Signer
{
    public static void SignPdfFiles()
    {
        ConsoleHelper.WriteHeader("✍️ PDF SIGNATURE ADDER ✍️");

        // Ask for signature image
        ConsoleHelper.WriteSubHeader("Signature Image");
        string signaturePath;
        while (true)
        {
            ConsoleHelper.WritePrompt("Enter the path to your signature PNG file: ");
            signaturePath = Console.ReadLine()?.Trim().Trim('"') ?? string.Empty;

            if (string.IsNullOrWhiteSpace(signaturePath))
            {
                ConsoleHelper.WriteWarning("Path cannot be empty. Please try again.");
                continue;
            }

            if (!File.Exists(signaturePath))
            {
                ConsoleHelper.WriteError($"File not found: {signaturePath}");
                continue;
            }

            if (!signaturePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelper.WriteWarning("File must be a PNG image. Please try again.");
                continue;
            }

            ConsoleHelper.WriteSuccess($"Signature loaded: {Path.GetFileName(signaturePath)}");
            break;
        }

        // Ask user to choose input mode
        ConsoleHelper.WriteSubHeader("Input Mode");
        Console.WriteLine();
        ConsoleHelper.WriteMenuOption("1", "Sign all PDF files from a folder");
        ConsoleHelper.WriteMenuOption("2", "Select specific PDF files to sign");
        Console.WriteLine();
        ConsoleHelper.WritePrompt("Choose input mode (1 or 2): ");

        var inputMode = Console.ReadLine()?.Trim();
        var filePaths = new List<string>();
        string? inputDirectory = null;

        if (inputMode == "1")
        {
            filePaths = GetFilesFromFolder(out inputDirectory);
        }
        else if (inputMode == "2")
        {
            filePaths = GetIndividualFiles();
        }
        else
        {
            ConsoleHelper.WriteError("Invalid option selected.");
            return;
        }

        if (filePaths.Count == 0)
        {
            ConsoleHelper.WriteError("No PDF files selected.");
            return;
        }

        // Ask for pages to sign
        ConsoleHelper.WriteSubHeader("Page Selection");
        ConsoleHelper.WriteInfo("Enter page numbers separated by comma (e.g., 1,3,5,7)");
        ConsoleHelper.WriteInfo("Or press Enter to sign ALL pages in each PDF.");
        ConsoleHelper.WritePrompt("Pages to sign: ");

        var pageInput = Console.ReadLine()?.Trim();
        List<int>? specificPages = null;

        if (!string.IsNullOrWhiteSpace(pageInput))
        {
            specificPages = [];
            var parts = pageInput.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in parts)
            {
                if (int.TryParse(part, out int pageNum) && pageNum >= 1)
                {
                    if (!specificPages.Contains(pageNum))
                    {
                        specificPages.Add(pageNum);
                    }
                }
                else
                {
                    ConsoleHelper.WriteWarning($"Invalid page number '{part}', skipping.");
                }
            }

            if (specificPages.Count == 0)
            {
                ConsoleHelper.WriteInfo("No valid pages specified. Will sign ALL pages.");
                specificPages = null;
            }
            else
            {
                specificPages.Sort();
                ConsoleHelper.WriteSuccess($"Will sign pages: {string.Join(", ", specificPages)}");
            }
        }
        else
        {
            ConsoleHelper.WriteInfo("Will sign ALL pages in each PDF.");
        }

        // Ask for signature position
        ConsoleHelper.WriteSubHeader("Signature Position");
        ConsoleHelper.WriteInfo("Specify the position from the bottom-right corner of each page.");

        // Distance from right
        double distanceFromRight;
        while (true)
        {
            ConsoleHelper.WritePrompt("Distance from right edge (in points, default 50): ");
            var rightInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(rightInput))
            {
                distanceFromRight = 50;
                break;
            }

            if (double.TryParse(rightInput, out distanceFromRight) && distanceFromRight >= 0)
            {
                break;
            }
            ConsoleHelper.WriteWarning("Please enter a valid positive number.");
        }

        // Distance from bottom
        double distanceFromBottom;
        while (true)
        {
            ConsoleHelper.WritePrompt("Distance from bottom edge (in points, default 50): ");
            var bottomInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(bottomInput))
            {
                distanceFromBottom = 50;
                break;
            }

            if (double.TryParse(bottomInput, out distanceFromBottom) && distanceFromBottom >= 0)
            {
                break;
            }
            ConsoleHelper.WriteWarning("Please enter a valid positive number.");
        }

        // Signature size
        double signatureWidth;
        while (true)
        {
            ConsoleHelper.WritePrompt("Signature width (in points, default 100): ");
            var widthInput = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(widthInput))
            {
                signatureWidth = 100;
                break;
            }

            if (double.TryParse(widthInput, out signatureWidth) && signatureWidth > 0)
            {
                break;
            }
            ConsoleHelper.WriteWarning("Please enter a valid positive number.");
        }

        ConsoleHelper.WriteInfo($"Position: {distanceFromRight}pt from right, {distanceFromBottom}pt from bottom");
        ConsoleHelper.WriteInfo($"Signature width: {signatureWidth}pt");

        // Ask for output directory
        ConsoleHelper.WriteSubHeader("Output Location");
        string outputDirectory;

        if (inputDirectory != null)
        {
            ConsoleHelper.WriteInfo($"Input folder: {inputDirectory}");
            ConsoleHelper.WritePrompt("Save signed PDFs to directory (press Enter to use input folder): ");
        }
        else
        {
            ConsoleHelper.WritePrompt("Save signed PDFs to directory: ");
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
                // Use the directory of the first file
                outputDirectory = Path.GetDirectoryName(filePaths[0]) ?? string.Empty;
                ConsoleHelper.WriteInfo($"Using source folder: {outputDirectory}");
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

        // Process PDFs
        ConsoleHelper.WriteSubHeader("Processing");
        ConsoleHelper.WriteInfo("Adding signatures to PDF files...");
        ConsoleHelper.WriteDivider();

        int totalFilesProcessed = 0;
        int totalPagesProcessed = 0;

        try
        {
            using var signatureImage = XImage.FromFile(signaturePath);

            // Calculate height maintaining aspect ratio
            var aspectRatio = signatureImage.PixelHeight / (double)signatureImage.PixelWidth;
            var signatureHeight = signatureWidth * aspectRatio;

            foreach (var filePath in filePaths)
            {
                try
                {
                    ConsoleHelper.WriteProgress($"Processing: {Path.GetFileName(filePath)}");

                    using var document = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);

                    int pagesSignedInFile = 0;

                    for (int i = 0; i < document.PageCount; i++)
                    {
                        int pageNumber = i + 1;

                        // Check if we should sign this page
                        if (specificPages != null && !specificPages.Contains(pageNumber))
                        {
                            continue;
                        }

                        // Skip if page number exceeds document pages
                        if (specificPages != null && pageNumber > document.PageCount)
                        {
                            continue;
                        }

                        var page = document.Pages[i];
                        using var gfx = XGraphics.FromPdfPage(page);

                        // Calculate position from bottom-right
                        var x = page.Width - distanceFromRight - signatureWidth;
                        var y = page.Height - distanceFromBottom - signatureHeight;

                        // Draw the signature
                        gfx.DrawImage(signatureImage, x, y, signatureWidth, signatureHeight);

                        pagesSignedInFile++;
                        totalPagesProcessed++;
                    }

                    // Save with timestamp
                    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    var originalName = Path.GetFileNameWithoutExtension(filePath);
                    var outputFileName = $"{timestamp}_{originalName}_Signed.pdf";
                    var outputPath = Path.Combine(outputDirectory, outputFileName);

                    document.Save(outputPath);
                    ConsoleHelper.WriteSuccess($"Signed {pagesSignedInFile} page(s) → {outputFileName}");
                    totalFilesProcessed++;
                }
                catch (Exception ex)
                {
                    ConsoleHelper.WriteError($"Failed to process {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            ConsoleHelper.WriteResultBox(
                "✓ SIGNING COMPLETED SUCCESSFULLY!",
                $"Files processed: {totalFilesProcessed}",
                $"Total pages signed: {totalPagesProcessed}",
                $"Signature size: {signatureWidth}x{signatureWidth * aspectRatio:F0}pt",
                $"Output location: {outputDirectory}"
            );
        }
        catch (Exception ex)
        {
            ConsoleHelper.WriteError($"Error loading signature image: {ex.Message}");
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

        // Ask for the count of files
        int fileCount;
        while (true)
        {
            ConsoleHelper.WritePrompt("How many PDF files would you like to sign? ");
            if (int.TryParse(Console.ReadLine(), out fileCount) && fileCount >= 1)
            {
                ConsoleHelper.WriteInfo($"You'll be signing {fileCount} PDF file(s).");
                break;
            }
            ConsoleHelper.WriteWarning("Please enter a valid number (at least 1 file required).");
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
