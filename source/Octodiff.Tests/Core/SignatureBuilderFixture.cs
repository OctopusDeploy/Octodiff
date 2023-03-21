using System.IO;
using NUnit.Framework;
using Octodiff.Core;
using Octodiff.Tests.Util;

namespace Octodiff.Tests.Core
{
    public class SignatureBuilderFixture
    {
        static byte[] BuildSignature(SignatureBuilder builder, byte[] input)
        {
            using (var inStream = new MemoryStream(input))
            using (var outStream = new MemoryStream())
            {
                builder.Build(inStream, new SignatureWriter(outStream));

                return outStream.ToArray();
            }
        }
        
        [Test]
        public void BuildSignatureWithDefaultSettingsAndTestData()
        {
            var builder = new SignatureBuilder();

            var result = BuildSignature(builder, Helpers.TestData);
            
            Assert.AreEqual("4f43544f5349470104534841310741646c657233323e3e3e0802f79fa2f0330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d", result.ToHexString());
        }
        
        [Test]
        public void BuildSignatureWithSmallChunkSize()
        {
            var builder = new SignatureBuilder
            {
                ChunkSize = SignatureBuilder.MinimumChunkSize // the smaller the chunk size, the larger the signature file needs to be
            };

            var result = BuildSignature(builder, Helpers.TestData);
            
            Assert.AreEqual("4f43544f5349470104534841310741646c657233323e3e3e8000951f26e719f3978cb607e80a9aab3abbcac8bb1ecbcecf3e80001f18260f0f73196c2aa57877ee5e31291a59b5afca4493658000e035f42a42c4a73471dea3b9746e22dd93893fd8549f11bd8000dd2ff46b72e00e30ecae4c70ee07721d221a3b8a6d1847fa08008a02860c21d4023a8ba580ecdba742e7400aa40b6e449bb3", result.ToHexString());
        }
        
        [Test]
        public void BuildSignatureWithLargeChunkSize()
        {
            var builder = new SignatureBuilder
            {
                ChunkSize = SignatureBuilder.MaximumChunkSize
            };

            // our input file is small so larger chunk size doesn't help here
            var result = BuildSignature(builder, Helpers.TestData);
            
            Assert.AreEqual("4f43544f5349470104534841310741646c657233323e3e3e0802f79fa2f0330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d", result.ToHexString());
        }
        
        [Test]
        public void BuildSignatureWithAdlerV2()
        {
            var builder = new SignatureBuilder
            {
                RollingChecksumAlgorithm = new Adler32RollingChecksumV2()
            };

            var result = BuildSignature(builder, Helpers.TestData);
            
            Assert.AreEqual("4f43544f5349470104534841310941646c6572333256323e3e3e0802f79fe5f8330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d", result.ToHexString());
        }
    }
}