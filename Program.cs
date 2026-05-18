using AmrAminPdfUtility.Utilities;

namespace AmrAminPdfUtility
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ConsoleHelper.WriteHeader("🔧 AMR AMIN PDF UTILITY 🔧");

            ConsoleHelper.WriteSubHeader("Main Menu");
            Console.WriteLine();
            ConsoleHelper.WriteMenuOption("1", "Merge multiple PDF files into one");
            ConsoleHelper.WriteMenuOption("2", "Extract pages from a PDF file");
            ConsoleHelper.WriteMenuOption("3", "Rotate pages in a PDF file");
            ConsoleHelper.WriteMenuOption("4", "Add signature to PDF pages");
            ConsoleHelper.WriteMenuOption("Q", "Quit application");
            Console.WriteLine();
            ConsoleHelper.WriteDivider();
            ConsoleHelper.WritePrompt("Select an option: ");

            var choice = Console.ReadLine()?.Trim().ToLower();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    Merger.MergePdfFiles();
                    break;
                case "2":
                    Extractor.ExtractPdfFiles();
                    break;
                case "3":
                    Rotator.RotatePdfPages();
                    break;
                case "4":
                    Signer.SignPdfFiles();
                    break;
                case "q":
                    ConsoleHelper.WriteGoodbye();
                    return;
                default:
                    ConsoleHelper.WriteError("Invalid option. Please run the application again.");
                    break;
            }

            ConsoleHelper.WriteGoodbye();
        }
    }
}
