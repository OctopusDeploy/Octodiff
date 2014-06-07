using System;

namespace Octodiff.CommandLine
{
    public class CommandException : Exception
    {
        public CommandException(string message)
            : base(message)
        {
        }
    }
}