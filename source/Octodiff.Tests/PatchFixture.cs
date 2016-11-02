using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class PatchFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void PatchingShouldResultInPerfectCopy(string name, int numberOfFiles)
        {
            var newName = Path.ChangeExtension(name, "2.zip");
            var copyName = Path.ChangeExtension(name, "2_out.zip");
            PackageGenerator.GeneratePackage(name, numberOfFiles);
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            Run("signature " + name + " " + name + ".sig");
            Run("delta " + name + ".sig " + newName + " " + name + ".delta");
            Run("patch " + name + " " + name + ".delta" + " " + copyName);
            Assert.That(ExitCode, Is.EqualTo(0));

            Assert.That(Sha1(newName), Is.EqualTo(Sha1(copyName)));
        }

        [Test]
        //[TestCase("SmallPackage1mb.zip", 10)] temp disable this passes locally but fails in appveyor?
        [TestCase("SmallPackage10mb.zip", 100)]
        public void PatchVerificationShouldFailWhenFilesModified(string name, int numberOfFiles)
        {
            var newBasis = Path.ChangeExtension(name, "1.zip");
            var newName = Path.ChangeExtension(name, "2.zip");
            var copyName = Path.ChangeExtension(name, "2_out.zip");
            PackageGenerator.GeneratePackage(name, numberOfFiles);
            PackageGenerator.ModifyPackage(name, newBasis, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            Run("signature " + name + " " + name + ".sig");
            Run("delta " + name + ".sig " + newName + " " + name + ".delta");
            Run("patch " + newBasis + " " + name + ".delta" + " " + copyName);
            Assert.That(ExitCode, Is.EqualTo(4));
            Assert.That(Output, Does.Contain("Error: Verification of the patched file failed"));
        }

        [Test]
        [TestCase("SmallPackage10mb.zip", 100)]
        public void PatchVerificationCanBeSkipped(string name, int numberOfFiles)
        {
            var newBasis = Path.ChangeExtension(name, "1.zip");
            var newName = Path.ChangeExtension(name, "2.zip");
            var copyName = Path.ChangeExtension(name, "2_out.zip");
            PackageGenerator.GeneratePackage(name, numberOfFiles);
            PackageGenerator.ModifyPackage(name, newBasis, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            Run("signature " + name + " " + name + ".sig");
            Run("delta " + name + ".sig " + newName + " " + name + ".delta");
            Run("patch " + newBasis + " " + name + ".delta" + " " + copyName + " --skip-verification");
            Assert.That(ExitCode, Is.EqualTo(0));
            Assert.That(Sha1(newName), Is.Not.EqualTo(Sha1(copyName)));
        }

        static string Sha1(string fileName)
        {
            using (var s = new FileStream(fileName, FileMode.Open))
            {
                return BitConverter.ToString(SHA1.Create().ComputeHash(s)).Replace("-", "");
            }
        }
    }
}