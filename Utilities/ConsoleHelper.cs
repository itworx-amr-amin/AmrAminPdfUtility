namespace AmrAminPdfUtility.Utilities;

public static class ConsoleHelper
{
    public static void WriteHeader(string title)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine($"║{CenterText(title, 64)}║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
    }

    public static void WriteSubHeader(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  ┌─ {text} ─┐");
        Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✓ {message}");
        Console.ResetColor();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  ✗ {message}");
        Console.ResetColor();
    }

    public static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  ⚠ {message}");
        Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"  ℹ   {message}");
        Console.ResetColor();
    }

    public static void WriteProgress(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"    → {message}");
        Console.ResetColor();
    }

    public static void WritePrompt(string message)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"  › {message}");
        Console.ResetColor();
    }

    public static void WriteMenuOption(string key, string description)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($"    [{key}] ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(description);
        Console.ResetColor();
    }

    public static void WriteDivider()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  ─────────────────────────────────────────────────────────────");
        Console.ResetColor();
    }

    public static void WriteResultBox(string title, params string[] lines)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ┌────────────────────────────────────────────────────────────┐");
        Console.WriteLine($"  │ {title,-58} │");
        Console.WriteLine("  ├────────────────────────────────────────────────────────────┤");
        foreach (var line in lines)
        {
            var truncated = line.Length > 56 ? line[..53] + "..." : line;
            Console.WriteLine($"  │   {truncated,-56} │");
        }
        Console.WriteLine("  └────────────────────────────────────────────────────────────┘");
        Console.ResetColor();
    }

    public static void WriteGoodbye()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  ★ Thank you for using Amr Amin PDF Utility! ★");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width) return text[..width];
        var padding = (width - text.Length) / 2;
        return text.PadLeft(text.Length + padding).PadRight(width);
    }
}
