using System;
namespace ConsoleApp.Framework;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class RequireArgumentAttribute(string name, string description, bool required = true) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public bool Required { get; } = required;
}