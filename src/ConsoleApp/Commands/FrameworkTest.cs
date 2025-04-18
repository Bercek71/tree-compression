using System.ComponentModel;
using ConsoleApp.Framework;
using ConsoleApp.Utils;
using TreeCompressionAlgorithms;
using TreeCompressionAlgorithms.CompressionStrategies.TreeRePair;
using Spectre.Console;

namespace ConsoleApp.Commands;

[Description("Framework testing command")]
public class FrameworkTest : ICommand
{
    [Argument("input", "The input string to compress")]
    private string InputFile { get; set; }

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

                var processTimer = new ProcessTimer(ctx);
                var compressor = new NaturalLanguageTreeCompressing(new TreeRepairOptimizedStrategy(maxN: 10), processTimer);

                var compressedTree = compressor.Compress(testingSentence);

                var decompressedText = compressor.Decompress(compressedTree);
                ctx.Status("Completed.");                

                // if(processTimer.Node?.ToString() != decompressedText)
                // {
                //     AnsiConsole.MarkupLine("[red]Error:[/] Decompressed text does not match original.");
                //     throw new InvalidOperationException("Decompressed text does not match original.");
                // }
                var encoder = new DepthFirstEncoder();

                AnsiConsole.Write(new Rule("[bold green]Compression Summary[/]").RuleStyle("grey"));
                //how to 

                if (processTimer.Node != null)
                {
                    var table = new Table()
                        .Border(TableBorder.Rounded)
                        .AddColumn("Metric")
                        .AddColumn("Value")
                        .AddRow("Original Length", testingSentence.Length.ToString())
                        .AddRow("Decompressed Length", decompressedText.Length.ToString())
                        .AddRow("Encoded tree size", encoder.EncodeTree(processTimer.Node).ToList().Sum(x => x.Length).ToString())
                        .AddRow("Compression Ratio", 
                            $"{(double)compressedTree.GetSize() / testingSentence.Length:F3}");

                    AnsiConsole.Write(table);
                }

                AnsiConsole.MarkupLine("\n[bold green]Decompressed Text Preview:[/]");
                AnsiConsole.WriteLine(decompressedText[..Math.Min(500, decompressedText.Length)] + "...");
            });
    }
}