
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
            //"EnglishMorphoditaDict"
            //"ParsitoTest"
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
        
        //check if type is marked as obsolete
        var obsoleteAttribute = type.GetCustomAttribute<ObsoleteAttribute>();
        if(obsoleteAttribute != null)
        {
            Console.WriteLine($"Command {command} is marked as obsolete: {obsoleteAttribute.Message}");
            return;
        }
        
        var commandInstance = (ICommand) Activator.CreateInstance(type)!;

        commandInstance.Execute(args);
        
        #endregion

    }
    
}