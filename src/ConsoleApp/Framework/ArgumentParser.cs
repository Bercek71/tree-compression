using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConsoleApp.Framework
{
    public class ArgumentParser
    {
        private readonly Dictionary<string, string?> _argDict;

        private ArgumentParser(string[] args)
        {
            _argDict = args
                .Select((val, index) => new { val, index })
                .Where(a => a.val.StartsWith("--"))
                .ToDictionary(a => a.val.TrimStart('-'), a => (a.index + 1 < args.Length ? args[a.index + 1] : null));
        }

        private string? GetArgumentValue(string name)
        {
            return _argDict.GetValueOrDefault(name);
        }

        public static void BindArguments(object target, string[] args)
        {
            var parser = new ArgumentParser(args);
            var properties = target.GetType().GetProperties();

            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<ArgumentAttribute>();
                if (attr == null) continue;
                var value = parser.GetArgumentValue(attr.Name);
                if (value != null)
                {
                    prop.SetValue(target, value);
                }
                else if (attr.Required)
                {
                    Console.WriteLine($"Missing required argument: --{attr.Name} ({attr.Description})");
                    new Help().Execute();
                }
            }
        }
    }
}