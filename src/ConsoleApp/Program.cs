
using System.Diagnostics;
using System.Reflection;
using ConsoleApp.Framework;
using MorphoDita;
using UDPipeBindings;
using ICommand = System.Windows.Input.ICommand;

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
            
            
            //"FrameworkTest",
            //"--input",
            //"Resources/Texts/old-man-and-the-sea.txt",
            
            "Benchmark"
            
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
            command?.Execute();

    }
    

    private static void LoadLibraries()
    {
        MorphoDitaLoader.LoadNativeLibrary();
        UdPipeLoader.LoadNativeLibrary();
    }
    
}