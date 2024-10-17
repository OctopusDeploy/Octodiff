using NUnit.Framework;
using NUnit.Framework.Legacy;
using Octodiff.Core;
using Octodiff.Tests.Util;

namespace Octodiff.Tests.Core
{
    [TestFixture]
    public class Adler32RollingChecksumV2Fixture
    {
        readonly Adler32RollingChecksumV2 c = new Adler32RollingChecksumV2();
        
        [Test]
        public void Name()
        {
            ClassicAssert.AreEqual("Adler32V2", c.Name);
        }
        
        [Test]
        public void Calculate()
        {
            var block = Helpers.TestData();
            
            ClassicAssert.AreEqual(2760448612, c.Calculate(block, 0, 100));
            ClassicAssert.AreEqual(2892962471, c.Calculate(block, 1, 100));
            ClassicAssert.AreEqual(2481658437, c.Calculate(block, 2, 100));
            ClassicAssert.AreEqual(595858050, c.Calculate(block, 93, 100));
            ClassicAssert.AreEqual(4175798263, c.Calculate(block, 0, block.Length));
            
            var largeBlock = Helpers.GenerateTestData(100 * 1024);
            ClassicAssert.AreEqual(180621253, c.Calculate(largeBlock, 0, largeBlock.Length));
        }
        
        [Test]
        public void Rotate()
        {
            ClassicAssert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 8));
            ClassicAssert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 16));
            ClassicAssert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 24));
            ClassicAssert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 32));
            
            ClassicAssert.AreEqual(3577289570, c.Rotate(3209698067, 0xAF, 0xFE, 8));
            ClassicAssert.AreEqual(3485539170, c.Rotate(3209698067, 0xAF, 0xFE, 16));
            ClassicAssert.AreEqual(3393788770, c.Rotate(3209698067, 0xAF, 0xFE, 24));
            ClassicAssert.AreEqual(3302038370, c.Rotate(3209698067, 0xAF, 0xFE, 32));
        }
    }

}