using System;
using System.IO;
using System.Security.Cryptography;
using NUnit.Framework;
using Octodiff.Core;
using Octodiff.Tests.Util;

namespace Octodiff.Tests
{
    [TestFixture]
    public class StreamingPatchFixture : CommandLineFixture
    {
        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void StreamingPatchingShouldResultInPerfectCopy(string name, int numberOfFiles)
        {
            var newName = Path.ChangeExtension(name, "2.zip");
            var copyName = Path.ChangeExtension(name, "2_out.zip");
            PackageGenerator.GeneratePackage(name, numberOfFiles);
            PackageGenerator.ModifyPackage(name, newName, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            Run("signature " + name + " " + name + ".sig");
            Run("delta " + name + ".sig " + newName + " " + newName + ".delta");

            // Patch
            // todo: update this to use the command version when available
            using (var basisStream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newNameDeltaFileStream = new FileStream(newName + ".delta", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var outStream = new FileStream(copyName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                var newNameDelta = new BinaryDeltaStream(basisStream, newNameDeltaFileStream);
                newNameDelta.Apply(outStream);
            }

            Assert.That(Sha1(newName), Is.EqualTo(Sha1(copyName)));
        }

        [Test]
        [TestCase("SmallPackage1mb.zip", 10)]
        [TestCase("SmallPackage10mb.zip", 100)]
        [TestCase("SmallPackage100mb.zip", 1000)]
        public void SecondLevelStreamingPatchingShouldResultInPerfectCopy(string name, int numberOfFiles)
        {
            var newName2 = Path.ChangeExtension(name, "2.zip");
            var newName3 = Path.ChangeExtension(name, "3.zip");
            var copyName = Path.ChangeExtension(name, "3_out.zip");
            PackageGenerator.GeneratePackage(name, numberOfFiles);
            PackageGenerator.ModifyPackage(name, newName2, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));
            PackageGenerator.ModifyPackage(newName2, newName3, (int)(0.33 * numberOfFiles), (int)(0.10 * numberOfFiles));

            Run("signature " + name + " " + name + ".sig");
            Run("delta " + name + ".sig " + newName2 + " " + newName2 + ".delta");

            Run("signature " + newName2 + " " + newName2 + ".sig");
            Run("delta " + newName2 + ".sig " + newName3 + " " + newName3 + ".delta");

            // Patch
            // todo: update this to use the command version when available
            using (var basisStream = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newName2DeltaFileStream = new FileStream(newName2 + ".delta", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var newName3DeltaFileStream = new FileStream(newName3 + ".delta", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var outStream = new FileStream(copyName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            {
                var newName2Delta = new BinaryDeltaStream(basisStream, newName2DeltaFileStream);
                Assert.True(newName2Delta.VerifyHashInMemory(), "The first delta passed verification");
                var newName3Delta = new BinaryDeltaStream(newName2Delta, newName3DeltaFileStream);
                Assert.True(newName3Delta.VerifyHashInMemory(), "The second delta passed verification");
                newName3Delta.Apply(outStream);
            }

            Assert.That(Sha1(newName3), Is.EqualTo(Sha1(copyName)));
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