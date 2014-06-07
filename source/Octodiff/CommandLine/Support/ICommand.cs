using System.IO;

namespace Octodiff.CommandLine.Support
{
    public interface ICommand
    {
        void GetHelp(TextWriter writer);
        int Execute(string[] commandLineArguments);
    }
}
