using System.Runtime.ExceptionServices;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class HelpFixture : CommandLineFixture
    {
        [Test]
        [TestCase(4, "")]
        [TestCase(0, "help")]
        [TestCase(4, "foo")]
        public void ShouldPrintGeneralHelp(int exitCode, string args)
        {
            Run(args);
            Assert.That(ExitCode, Is.EqualTo(exitCode));
            Assert.That(Output, Is.StringContaining("Usage: Octodiff <command>"));
            Assert.That(Output, Is.Not.StringContaining("Error"));
        }
        
        [Test]
        [TestCase(0, "help signature", "signature")]
        [TestCase(0, "help delta", "delta")]
        [TestCase(0, "help patch", "patch")]
        [TestCase(0, "help explain-delta", "explain-delta")]
        public void ShouldPrintCommandHelp(int exitCode, string args, string commandName)
        {
            Run(args);
            Assert.That(ExitCode, Is.EqualTo(exitCode));
            Assert.That(Output, Is.StringContaining("Usage: Octodiff " + commandName));
            Assert.That(Output, Is.StringContaining("Usage: Octodiff " + commandName));
            Assert.That(Output, Is.Not.StringContaining("Error"));
        }

        [Test]
        [TestCase(4, "signature", "No basis file was specified")]
        [TestCase(4, "delta", "No signature file was specified")]
        [TestCase(4, "delta foo.sig", "No new file was specified")]
        [TestCase(4, "patch", "No basis file was specified")]
        [TestCase(4, "patch foo.nupkg", "No delta file was specified")]
        [TestCase(4, "patch foo.nupkg foo.delta", "No new file was specified")]
        public void ShouldPrintHelpWhenAllArgumentsAreNotSpecified(int exitCode, string args, string text)
        {
            Run(args);
            Assert.That(ExitCode, Is.EqualTo(exitCode));
            Assert.That(Output, Is.StringContaining("Usage"));
            Assert.That(Output, Is.StringContaining("Error: " + text));
        }
    }
}