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
        public void BuildSignatureWithDefaultSettingsOverLargeFile()
        {
            var builder = new SignatureBuilder();

            var testData = Helpers.GenerateTestData(100 * 1024);
            File.WriteAllBytes("c:\\temp\\100ktestdata-csharp.bin", testData);
            
            var result = BuildSignature(builder,  Helpers.GenerateTestData(100*1024));
  
            Assert.AreEqual("4f43544f5349470104534841310741646c657233323e3e3e00080a73b304debbdddd84c899bd9e2cfc5bc2a8bd2adf4d10430008b470604864abf8d34217d6104c6c3e0a173283bce0cc4e8f00085f7932dfd1b95cb3b7c7e3055c90e65467f45497b5c4aadd0008cb71f9f6035eaa709a578e9ac330900410d2753f6ea13d6e00084e72ec239713299d78ea5ea7acd1c84e994ce36f2ee417bb00088171da145be16aacb6ec6e06f0918eac4071760941489da200081a6f85016ccac9c6156923281c2243d40ae6e9bf4b816125000871766eb03307131bc3fb5564d1af5473a7d7b6114bc8a0790008677b41c43d9eed5159c91e7838a90267801ccb5de24ac8b700080e775b2aa6136b72d6dea02464242c5b62eb718d8cd03d6e0008a97aab6682fb7203687b4ca2e7c439f08358a837b95b2d020008c279c99b791abccfb1995cd9a4a66ac3569f9c582e2e7c5d00088c7a86007960c8e890409c8963521738da72fa0d4610744b000844771558315ec5849209629adcd8c94b7e1f2d61880077880008ec7a48c9178db9ca004206a3c26ff417c612b2776cef74d50008d2731d6d1820d2c9fb133a29d0f52d2e82a03a1c59e20cd800085274e6f1ba27e3bc91511330bb5fce4989e5690926e69aea000875708ce1819797a0719d79c675b7f80613cebedfb1441ad500089077a4a7386ed9382e28bb43c082147052f2528953d73946000808730a06145d4895a5653d391fec9657b4742b98d2cba8af0008bf71f44ca07a1551f4a0bc897332dbd57e590853c585216600080974ebc01c988ca3f5eed1bee8dfe37d0b5025ea07b043250008226e2d45b34d4a7ca0763002b12713ce7e2aabf8bf7460e70008f07458ed5bd2b4958d78ce2ca58ef3e07c6d9d5b1544d59800080d7a0e31b4b1028185ae1a971ce79eda4a548ef1e49574b40008d4781b544c5738d6a486f9aa4ca3ca4a349a554262a54dee00088d79116fed45d69653de7d74a40ee2cb286922a990746a320008c0794f80c1323f60cdeb2a2003a6f0abde692c7910ecbb5900086c7b240323b5dee30fdc4223fdcab87aa78b6ac0a24b0ccb00089b77f53777a5e98ea8d6bf11af7168beeb48516871ab666a00086c7a951224931f3a42d2cb069297d41557d233e6b3c943a9000896729f749a2cc3a1306c8d0f94f4f8559a695ba6528ab76d0008ed77a7f2b51cdb799df0705fd44efd0801f93c26c8a8482400082f706cdf8e5a52547d3d003328a464df803e249dd749d60c0008d8749cfddc13b07bb547e1f29943afb03af7c50f4719ba7300084575d50c706878b92b9234309dc507c3e590fea594f2e6f200089572325e9b64675ce94e24956ad2b74ec2cd6e787e21ff2a000818744032610643f7d90c9e16f354e1a705fcb8eaeec01baa00082f6e3b1214540a6e65bb259491ca2084b073862889dfcd7b000862727076186090ba6594f0f1de9f1221dfc2ee1f757a6e1300083179495f8aba759a17c34528009fbdef9e568187efb7812b00083778b057656980acb3ff099c9441d0685f1ecb44b9a7d02c0008ff795a64c70954144a1999ebf8e6538111f1c8a763b5c6d40008517aa7e935f9818d49a3f9aae5bc35b4087dab04849f432600089c7a51ea69d5f2c63f47d912b7b0c1df967f2503d00969b500089a78698c6d644eb1225e35ed4d482c2165fc194569a1d79d0008ca7924791fabfd7725fe1ad8f59719246c06e56405188ca2000898745ebbece121d59349ac31a12ee9a7f61579307008b58500083777fc439b3d465debb1afd76647664939b10beb030c90cf0008a37257b6b454502c707c573fa7fc80c1685273e54b2a4f76", result.ToHexString());
        }
        
        [Test]
        public void BuildSignatureWithLargeChunkSizeOverLargeFile()
        {
            var builder = new SignatureBuilder
            {
                ChunkSize = SignatureBuilder.MaximumChunkSize
            };

            var result = BuildSignature(builder, Helpers.GenerateTestData(100*1024));
            
            Assert.AreEqual("4f43544f5349470104534841310741646c657233323e3e3e007cb823382f5470f51bab46eeb3913379e7b70a0d7329a9afce007cb5278ac69c31becd9bcd36f9afbd350ec15f4c437fd0cb67007c7a20e05ec605af9c2fd5a61b60f65600f5849f6ce1c53cf1001cac9ce9f194d25de18f219fa7832df14593cade50d8b0d2a2", result.ToHexString());
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