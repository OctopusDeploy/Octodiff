using System;
using System.IO;
using NUnit.Framework;
using Octodiff.Core;
using Octodiff.Tests.Util;

namespace Octodiff.Tests.Core
{
    public class BinaryDeltaWriterFixture
    {
        static byte[] WithDeltaWriter(Action<BinaryDeltaWriter> action)
        {
            using (var ms = new MemoryStream())
            {
                var b = new BinaryDeltaWriter(ms);
                action(b);
                return ms.ToArray();
            }
        }
        
        [Test]
        public void WritesHeader()
        {
            var output = WithDeltaWriter(b =>
            {
                // we only support sha1 so there's only one thing to test really
                b.WriteMetadata(SupportedAlgorithms.Hashing.Create("SHA1"), Helpers.GenerateTestData(20));
            });

            Assert.AreEqual("4f43544f44454c54410104534841311400000030820204308201aba003020102021418d83f07713e3e3e", output.ToHexString());
        }
        
        [Test]
        public void WritesCopyCommand()
        {
            var output = WithDeltaWriter(b =>
            {
                b.WriteCopyCommand(new DataRange(startOffset: 315412, length: 9874563)); // deliberately > 255 so we cross a single byte boundary and detect endianness problems
            });

            Assert.AreEqual("6014d004000000000083ac960000000000", output.ToHexString());
        }
        
        [Test]
        public void WritesDataCommand()
        {
            var output = WithDeltaWriter(b =>
            {
                var sourceFile = new MemoryStream(Helpers.GenerateTestData(1024));
                
                b.WriteDataCommand(sourceFile, 337, 515); // deliberately > 255 so we cross a single byte boundary and detect endianness problems
            });

            Assert.AreEqual("8003020000000000000931ecf7f3bd4bce212cfd2cbaa3533051301d0603551d0e04160414badd278a31e012776afbfda4ead8fdce904f0efc301f0603551d23041830168014badd278a31e012776afbfda4ead8fdce904f0efc300f0603551d130101ff040530030101ff300a06082a8648ce3d04030203470030440220599cef920115b64a7d0bc7de55a84bba7f05ee78b9e903af7cb52b4a5dcc8ea2022006575445dab9c21325a48de3bd7ce51a34612015a74648787c7a7e032645377030820204308201aba003020102021418d83f07718be4121df0a18d7610faf8d7a3bec4300a06082a8648ce3d0403023058310b30090603550406130241553113301106035504080c0a536f6d652d537461746531173015060355040a0c0e4f63746f707573204465706c6f79310c300a060355040b0c03522644310d300b06035504030c0454455354301e170d3233303332303039343834325a170d3234303331393039343834325a3058310b30090603550406130241553113301106035504080c0a536f6d652d537461746531173015060355040a0c0e4f63746f707573204465706c6f79310c300a060355040b0c03522644310d300b06035504030c04544553543059301306072a8648ce3d020106082a8648ce3d03010703420004504b77248d83e2e3e209bbb2297a0e4d24ff45e79eff88dd165e6419ae98512dabd2219da46e93d7ff98d5a1cb80", output.ToHexString());
        }
    }

}