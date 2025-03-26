using System.Reflection;

namespace ConsoleApp.Framework
{
    public static class CommandRegistry
    {
        private static readonly Dictionary<string, Type> Commands = new();

        static CommandRegistry()
        {
            RegisterCommands(Assembly.GetExecutingAssembly());
        }

        private static void RegisterCommands(Assembly assembly)
        {
            var commandTypes = assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ICommand).IsAssignableFrom(t));

            foreach (var type in commandTypes)
            {
                Commands[type.Name.ToLower()] = type;
            }
        }

        public static ICommand? CreateCommand(string commandName, string[] args)
        {
            if (!Commands.TryGetValue(commandName.ToLower(), out var commandType))
            {
                Console.WriteLine($"Unknown command: {commandName}");
                new Help().Execute();
                return null;
                //throw new ArgumentException($"Command {commandName} not found");
            }

            var commandInstance = Activator.CreateInstance(commandType);
            if (commandInstance == null) return null;

            ArgumentParser.BindArguments(commandInstance, args);
            return (ICommand)commandInstance;

        }
    }
}