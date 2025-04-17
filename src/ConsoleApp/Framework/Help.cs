using System.ComponentModel;
using System.Reflection;
using Spectre.Console;

namespace ConsoleApp.Framework;

[Description("Displays help information for the commands")]
public class Help : ICommand
{
    //load all the commands and arguments from the assembly and return a description of the commands
    private static void DisplayHelp()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var types = assembly.GetTypes();
        var commands = types.Where(t => t.GetInterfaces().Contains(typeof(ICommand))).OrderBy(t => t.Name);

        // Display app name and usage without any complex markup
        AnsiConsole.WriteLine();
        
        // Create a styled rule for the header
        var rule = new Rule($"{assembly.GetName().Name} - Command Line Tool");
        rule.Style = Style.Parse("cyan");
        AnsiConsole.Write(rule);
        
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine($"Usage: {assembly.GetName().Name} <command> [arguments]");
        AnsiConsole.WriteLine();

        // Create a basic table
        var table = new Table();
        table.Border = TableBorder.Minimal;
        table.Expand();

        // Add columns
        table.AddColumn("Command");
        table.AddColumn("Description");

        // Add command rows and their arguments
        foreach (var command in commands)
        {
            var commandName = command.Name;
            var commandDescription = command.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "No description";

            // First add the command itself
            table.AddRow(commandName, commandDescription);

            // Then add all arguments indented
            var arguments = command.GetProperties().Where(p => p.GetCustomAttribute<ArgumentAttribute>() != null).ToList();
            
            foreach (var argument in arguments)
            {
                var argumentName = argument.GetCustomAttribute<ArgumentAttribute>()?.Name;
                var argumentDescription = argument.GetCustomAttribute<ArgumentAttribute>()?.Description;
                var argumentRequired = argument.GetCustomAttribute<ArgumentAttribute>()?.Required;
                
                var reqText = (bool)argumentRequired! ? "(required)" : "(optional)";
                
                table.AddRow($"  --{argumentName}", $"{argumentDescription} {reqText}");
            }

            // Add an empty row after each command
            if (commands.Last() != command)
            {
                table.AddEmptyRow();
            }
        }

        AnsiConsole.Write(table);

        // Create a styled rule for the footer
        var footerRule = new Rule();
        footerRule.Style = Style.Parse("cyan");
        AnsiConsole.Write(footerRule);
    }

    public void Execute()
    {
        DisplayHelp();
    }
}