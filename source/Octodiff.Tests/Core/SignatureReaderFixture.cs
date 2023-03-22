using System;
using System.IO;
using NUnit.Framework;
using Octodiff.Core;
using Octodiff.Diagnostics;
using Octodiff.Tests.Util;

namespace Octodiff.Tests.Core
{
    public class SignatureReaderFixture
    {
        static Signature ReadSignature(byte[] input)
        {
            using (var inStream = new MemoryStream(input))
            {
                var reader = new SignatureReader(inStream, NullProgressReporter.Instance);
                return reader.ReadSignature();
            }
        }

        static void AssertChunk(ChunkSignature chunk, UInt32 checksum, short length, string hashAsHexString)
        {
            Assert.AreEqual(checksum, chunk.RollingChecksum);
            Assert.AreEqual(length, chunk.Length);
            Assert.AreEqual(hashAsHexString, chunk.Hash.ToHexString());
        }

        [Test]
        public void ReadsStandardSignature()
        {
            var input =
                "4f43544f5349470104534841310741646c657233323e3e3e0802f79fa2f0330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d"
                    .HexStringToByteArray();

            var s = ReadSignature(input);
            Assert.AreEqual("SHA1", s.HashAlgorithm.Name);
            Assert.AreEqual("Adler32", s.RollingChecksumAlgorithm.Name);
            
            Assert.AreEqual(1, s.Chunks.Count);
            AssertChunk(s.Chunks[0], 4037189623, 520, "330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d");
        }
        
        [Test]
        public void ReadsStandardSignatureAdlerV2()
        {
            var input =
                "4f43544f5349470104534841310941646c6572333256323e3e3e0802f79fe5f8330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d"
                    .HexStringToByteArray();

            var s = ReadSignature(input);
            Assert.AreEqual("SHA1", s.HashAlgorithm.Name);
            Assert.AreEqual("Adler32V2", s.RollingChecksumAlgorithm.Name);
            
            Assert.AreEqual(1, s.Chunks.Count);
            AssertChunk(s.Chunks[0], 4175798263, 520, "330bd06982d3b5dbda6c1a6ad16687a0cdb03c0d");
        }
        
        [Test]
        public void ReadsSmallChunkSizeSignature()
        {
            var input =
                "4f43544f5349470104534841310741646c657233323e3e3e8000951f26e719f3978cb607e80a9aab3abbcac8bb1ecbcecf3e80001f18260f0f73196c2aa57877ee5e31291a59b5afca4493658000e035f42a42c4a73471dea3b9746e22dd93893fd8549f11bd8000dd2ff46b72e00e30ecae4c70ee07721d221a3b8a6d1847fa08008a02860c21d4023a8ba580ecdba742e7400aa40b6e449bb3"
                    .HexStringToByteArray();

            var s = ReadSignature(input);
            Assert.AreEqual("SHA1", s.HashAlgorithm.Name);
            Assert.AreEqual("Adler32", s.RollingChecksumAlgorithm.Name);
            
            Assert.AreEqual(5, s.Chunks.Count);
            AssertChunk(s.Chunks[0], 3878035349, 128, "19f3978cb607e80a9aab3abbcac8bb1ecbcecf3e");
            AssertChunk(s.Chunks[1], 254154783, 128, "0f73196c2aa57877ee5e31291a59b5afca449365");
            AssertChunk(s.Chunks[2], 720647648, 128, "42c4a73471dea3b9746e22dd93893fd8549f11bd");
            AssertChunk(s.Chunks[3], 1811165149, 128, "72e00e30ecae4c70ee07721d221a3b8a6d1847fa");
            AssertChunk(s.Chunks[4], 210109066, 8, "21d4023a8ba580ecdba742e7400aa40b6e449bb3");
        }
        
        [Test]
        public void ReadsLargeChunkSizeSignatureOverLargeFile()
        {
            var input =
                "4f43544f5349470104534841310741646c657233323e3e3e007cb823382f5470f51bab46eeb3913379e7b70a0d7329a9afce007cb5278ac69c31becd9bcd36f9afbd350ec15f4c437fd0cb67007c7a20e05ec605af9c2fd5a61b60f65600f5849f6ce1c53cf1001cac9ce9f194d25de18f219fa7832df14593cade50d8b0d2a2"
                    .HexStringToByteArray();

            var s = ReadSignature(input);
            Assert.AreEqual("SHA1", s.HashAlgorithm.Name);
            Assert.AreEqual("Adler32", s.RollingChecksumAlgorithm.Name);
            
            Assert.AreEqual(4, s.Chunks.Count);
            AssertChunk(s.Chunks[0], 792208312, 31744, "5470f51bab46eeb3913379e7b70a0d7329a9afce");
            AssertChunk(s.Chunks[1], 3330942901, 31744, "9c31becd9bcd36f9afbd350ec15f4c437fd0cb67");
            AssertChunk(s.Chunks[2], 1591746682, 31744, "c605af9c2fd5a61b60f65600f5849f6ce1c53cf1");
            AssertChunk(s.Chunks[3], 4058619052, 7168, "94d25de18f219fa7832df14593cade50d8b0d2a2");
        }
    }
}