using Spectre.Console;
using TreeCompressionPipeline.TreeStructure;

namespace TreeCompressionPipeline;

/// <summary>
/// Jednoduchá implementace observera pro sledování průběhu procesu. Slouží hlavně pro debugging a testování.
/// </summary>
public class ProcessMonitor : IProcessObserver
{
    public void OnStart(string process)
    {
        AnsiConsole.MarkupLine($"[bold green]Starting:[/] [underline]{process}[/]");
    }

    public void OnProgress(string process, double percentComplete)
    {
        AnsiConsole.MarkupLine($"[yellow]Progress:[/] {process} - [bold]{percentComplete:F1}%[/]");
    }

    public void OnComplete(string process, object result)
    {
        string? SafeMarkup(string input) => input?
            .Replace("[", "[[")
            .Replace("]", "]]");
        
        AnsiConsole.MarkupLine($"[bold blue]Process complete:[/] [underline]{process}[/]");
        AnsiConsole.WriteLine("Result:");
        AnsiConsole.Write(new Panel(SafeMarkup(result?.ToString() ?? "null") ?? string.Empty)
            .Border(BoxBorder.Rounded)
            .Header("Output", Justify.Center)
            .Collapse());
    }

    public void OnError(string process, Exception error)
    {
        AnsiConsole.MarkupLine($"[bold red]Error in {process}:[/] {error.Message}");
        AnsiConsole.WriteException(error, ExceptionFormats.ShortenEverything);
    }
    
    
}