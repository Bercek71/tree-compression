using System.ComponentModel;
using ConsoleApp.Framework;
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using TreeCompressionPipeline;
using Spectre.Console;

namespace ConsoleApp.Commands;

[Description("Framework testing command")]
public class FrameworkTest : ICommand
{
    [Argument("input", "The input string to compress")]
    public required string InputFile { get; set; }

    public void Execute()
    {
        AnsiConsole.MarkupLine($"[bold yellow]Input File:[/] [green]{InputFile}[/]");

        if (!File.Exists(InputFile))
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] File not found: [italic]{InputFile}[/]");
            throw new FileNotFoundException("The input file does not exist.", InputFile);
        }

        AnsiConsole.Status()
            .Start("Reading and compressing...", ctx =>
            {
                var testingSentence = File.ReadAllText(InputFile);
                AnsiConsole.MarkupLine($"[bold]Loaded sentence length:[/] [blue]{testingSentence.Length}[/]");

                var compressor = new NaturalLanguageTreeCompressing(new TreeRepairOptimizedStrategy(maxN: 10));

                ctx.Status("Compressing...");
                var compressedTree = compressor.Compress(testingSentence);

                ctx.Status("Decompressing...");
                var decompressedText = compressor.Decompress(compressedTree);

                AnsiConsole.Write(new Rule("[bold green]Compression Summary[/]").RuleStyle("grey"));

                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Metric")
                    .AddColumn("Value")
                    .AddRow("Original Length", testingSentence.Length.ToString())
                    .AddRow("Decompressed Length", decompressedText.Length.ToString())
                    .AddRow("Compressed Tree Size", compressedTree.Structure.Length.ToString())
                    .AddRow("Compression Ratio", 
                        $"{(double)compressedTree.Structure.Length / testingSentence.Length:F3}");

                AnsiConsole.Write(table);

                AnsiConsole.MarkupLine("\n[bold green]Decompressed Text Preview:[/]");
                AnsiConsole.WriteLine(decompressedText[..Math.Min(500, decompressedText.Length)] + "...");
            });
    }
}