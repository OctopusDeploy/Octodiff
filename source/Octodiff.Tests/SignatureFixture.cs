using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class SignatureFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void ShouldCreateSignature(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(name, numberOfFiles);

            Run("signature " + name + " " + name + ".sig");
            Assert.That(ExitCode, Is.EqualTo(0));

            var basisSize = new FileInfo(name).Length;
            var signatureSize = new FileInfo(name + ".sig").Length;
            var signatureSizePercentageOfBasis = signatureSize/(double) basisSize;

            Trace.WriteLine(string.Format("Basis size: {0:n0}", basisSize));
            Trace.WriteLine(string.Format("Signature size: {0:n0}", signatureSize));
            Trace.WriteLine(string.Format("Signature ratio: {0:n3}", signatureSizePercentageOfBasis));
            Assert.IsTrue(0.012 <= signatureSizePercentageOfBasis && signatureSizePercentageOfBasis <= 0.014);
        }
    }
}