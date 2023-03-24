using System.IO;
using NUnit.Framework;
using Octodiff.Core;
using Octodiff.Diagnostics;
using Octodiff.Tests.Util;

namespace Octodiff.Tests.Core
{
    public class DeltaBuilderFixture
    {
        static byte[] BuildDelta(byte[] newFile, byte[] signatureFile)
        {
            var d = new DeltaBuilder();

            using (var inputNewFileStream = new MemoryStream(newFile))
            using (var inputSigFileStream = new MemoryStream(signatureFile))
            using (var outputDeltaFileStream = new MemoryStream())
            {
                var sigReader = new SignatureReader(inputSigFileStream, NullProgressReporter.Instance);
                var deltaWriter = new BinaryDeltaWriter(outputDeltaFileStream);
                
                d.BuildDelta(inputNewFileStream, sigReader, deltaWriter);

                return outputDeltaFileStream.ToArray();
            }
        }
        
        // just helps us get test input data
        static byte[] BuildSignature(byte[] input)
        {
            using (var inStream = new MemoryStream(input))
            using (var outStream = new MemoryStream())
            {
                new SignatureBuilder().Build(inStream, new SignatureWriter(outStream));

                return outStream.ToArray();
            }
        }
        
        [Test]
        public void BuildsNoOpDeltaForSameInput()
        {
            var signature = BuildSignature(Helpers.TestData);

            var deltaFile = BuildDelta(Helpers.TestData, signature);
            Assert.AreEqual("4f43544f44454c544101045348413114000000330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d3e3e3e6000000000000000000802000000000000", deltaFile.ToHexString());
        }
    }
}