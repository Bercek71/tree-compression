
using System.Diagnostics;
using System.Reflection;
using MorphoDita;
using Ufal.MorphoDiTa;
using Version = Ufal.MorphoDiTa.Version;

namespace TreeCompressionMain;

public partial class Program
{

    private static void LoadLibraries()
    {
        MorphoDitaLoader.LoadNativeLibrary();
    }
    
    public static void Main(string[] args)
    {

        #if DEBUG
        //Custom args for debugging
        args =
        [
            //"EnglishMorphoditaTagger"
            "EnglishMorphoditaDict"
        ];
        #endif
        

        #region CommandExecution
        LoadLibraries();
        if(args.Length == 0)
        {
            Console.WriteLine("No command specified");
            return;
        }
        var command = args[0];
        var assembly = Assembly.GetExecutingAssembly();
        var type = assembly.GetType($"TreeCompressionMain.Commands.{command}");

        if(type == null)
        {
            Console.WriteLine($"Command {command} not found");
            return;
        }

        var commandInstance = (ICommand) Activator.CreateInstance(type)!;

        commandInstance.Execute(args);
        
        #endregion

    }
    
}