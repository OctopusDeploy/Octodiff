using System;
using System.IO;
using System.Linq;
using Octodiff.CommandLine.Support;
using Octodiff.Core;
using Octodiff.Diagnostics;

namespace Octodiff.CommandLine
{
    [Command("explain-delta", Description = "Prints instructions from a delta file; useful when debugging.", Usage = "<delta-file>")]
    class ExplainDeltaCommand : ICommand
    {
        private readonly OptionSet options;
        private string deltaFilePath;

        public ExplainDeltaCommand()
        {
            options = new OptionSet();
            options.Positional("delta-file", "The file to read the delta from.", v => deltaFilePath = v);
        }

        public void GetHelp(TextWriter writer)
        {
            options.WriteOptionDescriptions(writer);
        }

        public int Execute(string[] commandLineArguments)
        {
            options.Parse(commandLineArguments);

            if (string.IsNullOrWhiteSpace(deltaFilePath))
                throw new OptionException("No delta file was specified", "delta-file");

            deltaFilePath = Path.GetFullPath(deltaFilePath);

            if (!File.Exists(deltaFilePath))
            {
                throw new FileNotFoundException("File not found: " + deltaFilePath, deltaFilePath);
            }

            using (var deltaStream = new FileStream(deltaFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var reader = new BinaryDeltaReader(deltaStream, NullProgressReporter.Instance);

                reader.Apply((data, offset, count) =>
                {
                    if (count > 20)
                    {
                        Console.WriteLine("Data: ({0} bytes): {1}...", count,
                            BitConverter.ToString(data.Skip(offset).Take(20).ToArray()));
                    }
                    else
                    {
                        Console.WriteLine("Data: ({0} bytes): {1}", count, BitConverter.ToString(data.Skip(offset).Take(count).ToArray()));                        
                    }
                }, 
                (start, offset) => Console.WriteLine("Copy: {0:X} to {1:X}", start, offset));
            }

            return 0;
        }
    }
}