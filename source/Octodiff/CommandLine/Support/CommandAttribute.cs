using System;
using Octodiff.CommandLine.Support;

namespace Octodiff.CommandLine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CommandAttribute : Attribute, ICommandMetadata
    {
        public CommandAttribute(string name, params string[] aliases)
        {
            Name = name;
            Aliases = aliases;
        }

        public string Name { get; set; }
        public string[] Aliases { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
    }
}