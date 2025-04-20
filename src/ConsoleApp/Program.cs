using ConsoleApp.Framework;
using MorphoDita;
using Spectre.Console;
using UDPipeBindings;

namespace ConsoleApp;

public static class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        //Custom args for debugging
        args =
        [
            //"EnglishMorphoditaTagger"
            //"EnglishMorphoditaDict"
            //"ParsitoTest"
            //"UDPipeTest"


            "FrameworkTest",
            "--input",
            "Resources/Texts/prose/old-man-and-the-sea.txt",

            //"Resources/Texts/odyssey.txt",

            //"Benchmark"

            // "GenerateReport",
            // "--directory",
            // "Resources/Texts/",
        ];
#endif
        



        if (args.Length < 1)
        {
            new Help().Execute();
            return;
        }
        
        //ExecuteCommand(args);
        var commandName = args[0];
        var commandArgs = args.Skip(1).ToArray();
        var command = CommandRegistry.CreateCommand(commandName, commandArgs);
        if (command == null) return;
        LoadLibraries();
        try
        {
            command?.Execute();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]X Error executing command: [/]");
            AnsiConsole.WriteException(ex);
        }
    }


    private static void LoadLibraries()
    {
            AnsiConsole.Status().Start("Loading libraries...", ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("green"));
                ctx.Status("Loading MorphoDiTa...");
                try
                {
                    MorphoDitaLoader.LoadNativeLibrary();
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]X Error loading MorphoDiTa: [/]");
                    AnsiConsole.WriteException(ex);
                }

                ctx.Status("Loading UDPipe...");
                try
                {
                    UdPipeLoader.LoadNativeLibrary();
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]X Error loading UDPipe: [/]");
                    AnsiConsole.WriteException(ex);
                }

                ctx.Status("Libraries loaded.");
                AnsiConsole.MarkupLine("[green]✔ Libraries loaded[/]");
            });
    }
}