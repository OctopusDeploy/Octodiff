using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
            var path = GetExePath();
#if !NET40
            args = $"{path} {args}";
            path = "dotnet";
#endif
            var exit = SilentProcessRunner.ExecuteCommand(path,
                args,
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
#if NET40
            return new Uri(typeof(DeltaBuilder).Assembly.CodeBase).LocalPath;
#else
            return Path.Combine(Path.GetDirectoryName(new Uri(typeof(CommandLineFixture).GetTypeInfo().Assembly.CodeBase).LocalPath), "Octodiff.Tests.dll");
#endif
        }

        string GetCurrentDirectory()
        {
#if NET40
            return Environment.CurrentDirectory;
#else
            return System.IO.Directory.GetCurrentDirectory();
#endif
        }
    }
}
