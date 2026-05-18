using AmrAminPdfUtility.Utilities;

namespace AmrAminPdfUtility
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("==== Welcome to Amr Amin Pdf utilities ====\n\n\n");
            Console.WriteLine("1. Merge PDF files");
            Console.WriteLine("2. Extract PDF pages");
            Console.Write("Select an option: ");
            var choice = Console.ReadLine();
            Console.WriteLine("\n\n\n");

            switch (choice)
            {
                case "1":
                    Merger.MergePdfFiles();
                    break;
                case "2":
                    Extractor.ExtractPdfFiles();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }
}
