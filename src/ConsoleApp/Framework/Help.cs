using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace ConsoleApp.Framework;

public class Help : ICommand
{
    //load all the commands and arguments from the assembly and return a description of the commands
    private static string GetHelp()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();
        var commands = types.Where(t => t.GetInterfaces().Contains(typeof(ICommand)));
        var help = new StringBuilder();
        help.AppendLine($"Usage: {assembly.GetName().Name} <command> [options]");
        help.AppendLine("Commands:");
        foreach (var command in commands)
        {
            var commandName = command.Name;
            var commandDescription = command.GetCustomAttribute<DescriptionAttribute>()?.Description;
            help.AppendLine($"\t{commandName}: {commandDescription}");
            var arguments = command.GetProperties().Where(p => p.GetCustomAttribute<RequireArgumentAttribute>() != null);
            foreach (var argument in arguments)
            {
                var argumentName = argument.GetCustomAttribute<RequireArgumentAttribute>()?.Name;
                var argumentDescription = argument.GetCustomAttribute<RequireArgumentAttribute>()?.Description;
                var argumentRequired = argument.GetCustomAttribute<RequireArgumentAttribute>()?.Required;
                help.AppendLine($"\t\t --{argumentName}: {argumentDescription} {((bool)argumentRequired! ? "(required)" : "")}");
            }
        }

        return help.ToString();
    }

    public void Execute()
    {
        Console.WriteLine(GetHelp());
    }
}