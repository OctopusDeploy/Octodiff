using System;
using System.Diagnostics;
using System.Text;
using Octodiff.Core;
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
            var path = new Uri(typeof (DeltaBuilder).Assembly.CodeBase).LocalPath;
            var exit = SilentProcessRunner.ExecuteCommand(path,
                args,
                Environment.CurrentDirectory,
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
    }
}
