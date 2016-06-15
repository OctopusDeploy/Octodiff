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
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void ShouldCreateDifferentSignaturesBasedOnChunkSize(string name, int numberOfFiles)
        {
            PackageGenerator.GeneratePackage(name, numberOfFiles);

            Run("signature " + name + " " + name + ".sig.1 --chunk-size=128");
            Run("signature " + name + " " + name + ".sig.2 --chunk-size=256");
            Run("signature " + name + " " + name + ".sig.3 --chunk-size=1024");
            Run("signature " + name + " " + name + ".sig.4 --chunk-size=2048");
            Run("signature " + name + " " + name + ".sig.5 --chunk-size=31744");

            Assert.That(Length(name + ".sig.1") > Length(name + ".sig.2"));
            Assert.That(Length(name + ".sig.2") > Length(name + ".sig.3"));
            Assert.That(Length(name + ".sig.3") > Length(name + ".sig.4"));
            Assert.That(Length(name + ".sig.4") > Length(name + ".sig.5"));
        }

        static long Length(string fileName)
        {
            return new FileInfo(fileName).Length;
        }
    }
}