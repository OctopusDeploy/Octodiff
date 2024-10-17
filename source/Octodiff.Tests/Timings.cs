using System;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class TimingsFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void ExecuteWithTimings(string name, int numberOfFiles)
        {
            var newName = Path.ChangeExtension(name, "2.zip");
            var copyName = Path.ChangeExtension(name, "2_out.zip");

            Time("Package creation", () => PackageGenerator.GeneratePackage(name, numberOfFiles));
            Time("Package modification", () => PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles)));
            Time("Signature creation", () => Run("signature " + name + " " + name + ".sig"));
            Time("Delta creation", () => Run("delta " + name + ".sig " + newName + " " + name + ".delta"));
            Time("Patch application", () => Run("patch " + name + " " + name + ".delta" + " " + copyName));
            Time("Patch application (no verify)", () => Run("patch " + name + " " + name + ".delta" + " " + copyName + " --skip-verification"));
        }

        static void Time(string task, Action callback)
        {
            var watch = Stopwatch.StartNew();
            callback();
            Trace.WriteLine(task.PadRight(30, ' ') + ": " + watch.ElapsedMilliseconds + "ms");
        }
    }
}