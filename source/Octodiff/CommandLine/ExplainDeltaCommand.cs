using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Octodiff.CommandLine.Support;
using Octodiff.Core;
using Octodiff.Diagnostics;

namespace Octodiff.CommandLine
{
    [Command("explain-delta", Description = "Given a signature file and a new file, creates a delta file", Usage = "<signature-file> <new-file> [<delta-file>]")]
    public class ExplainDeltaCommand : ICommand
    {
        private readonly List<Action<DeltaBuilder>> configuration = new List<Action<DeltaBuilder>>();
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
                var reader = new BinaryDeltaReader(deltaStream, new NullProgressReporter());

                reader.Apply(data =>
                {
                    if (data.Length > 20)
                    {
                        Console.WriteLine("Data: ({0} bytes): {1}...", data.Length,
                            BitConverter.ToString(data.Take(20).ToArray()));
                    }
                    else
                    {
                        Console.WriteLine("Data: ({0} bytes): {1}", data.Length, BitConverter.ToString(data.ToArray()));                        
                    }
                },
                    copy: (long start, long offset) =>
                    {
                        Console.WriteLine("Copy: {0:X} to {1:X}", start, offset);
                    });
            }

            return 0;
        }
    }
}