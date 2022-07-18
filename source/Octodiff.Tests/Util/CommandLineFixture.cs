using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Octopus.Platform.Util;

namespace Octodiff.Tests.Util
{
    public abstract class CommandLineFixture
    {
        protected string StdErr { get; private set; }
        protected string StdOut { get; private set; }
        protected string Output { get; private set; }
        protected int ExitCode { get; set; }

        public void Run(string args)
        {
            var stdErrBuilder = new StringBuilder();
            var stdOutBuilder = new StringBuilder();
            var outputBuilder = new StringBuilder();
            var commandArgs = $"{GetExePath()} {args}";
            var exit = SilentProcessRunner.ExecuteCommand("dotnet",
                commandArgs,
                GetCurrentDirectory(),
                output =>
                {
                    stdOutBuilder.AppendLine(output);
                    outputBuilder.AppendLine(output);
                    Trace.WriteLine(output);
                },
                output =>
                {
                    stdErrBuilder.AppendLine(output);
                    outputBuilder.AppendLine(output);
                    Trace.WriteLine(output);
                });

            StdErr = stdErrBuilder.ToString();
            StdOut = stdOutBuilder.ToString();
            Output = outputBuilder.ToString();
            ExitCode = exit;
        }

        string GetExePath()
        {
            return Path.Combine(Path.GetDirectoryName(new Uri(typeof(CommandLineFixture).GetTypeInfo().Assembly.Location).LocalPath)!, "Octodiff.Tests.dll");
        }

        string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}
