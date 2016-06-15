﻿using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class DeltaFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void DeltaOfUnchangedFileShouldResultInJustCopySegment(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(name, numberOfFiles);

            Run("signature " + name + " " + name + ".sig");
            Assert.That(ExitCode, Is.EqualTo(0));

            Run("delta " + name + ".sig " + name + " " + name + ".delta");
            Assert.That(ExitCode, Is.EqualTo(0));

            Run("explain-delta " + name + ".delta");
            Assert.That(Regex.IsMatch(Output, "^Copy: 0 to ([0-9A-F]+)\r\n$"));
            Assert.That(Output, Is.Not.StringContaining("Data:"));
        }

        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void DeltaOfChangedFileShouldResultInNewDataSegments(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(name, numberOfFiles);

            Run("signature " + name + " " + name + ".sig");
            Assert.That(ExitCode, Is.EqualTo(0));

            var newName = Path.ChangeExtension(name, "2.zip");
            PackageGenerator.ModifyPackage(name, newName, (int) (0.33*numberOfFiles), (int) (0.10*numberOfFiles));

            Run("delta " + name + ".sig " + newName + " " + name + ".delta");
            Assert.That(ExitCode, Is.EqualTo(0));

            Run("explain-delta " + name + ".delta");
            Assert.That(Regex.IsMatch(Output, "Copy: ([0-9A-F]+) to ([0-9A-F]+)\r\n"));
            Assert.That(Regex.IsMatch(Output, "Data: \\(([0-9]+) bytes\\)"));

            var originalSize = new FileInfo(name).Length;
            var newSize = new FileInfo(newName).Length;
            var deltaSize = new FileInfo(name + ".delta").Length;
            var actualDifference = Math.Abs(newSize - originalSize);
            var deltaToActualRatio = (double) deltaSize/actualDifference;
            Trace.WriteLine(string.Format("Delta ratio: {0:n3}", deltaToActualRatio));
            Assert.IsTrue(deltaSize * 2 < newSize, "Delta should be at least half the new file size");
            Assert.IsTrue(0.80 <= deltaToActualRatio && deltaToActualRatio <= 1.60, "Delta should be pretty close to the actual file differences");
        }
    }
}