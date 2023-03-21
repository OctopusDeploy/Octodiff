using NUnit.Framework;
using Octodiff.Core;
using Octodiff.Tests.Util;

namespace Octodiff.Tests.Core
{
    [TestFixture]
    public class Adler32RollingChecksumFixture
    {
        Adler32RollingChecksum c = new Adler32RollingChecksum();
        
        [Test]
        public void Name()
        {
            Assert.AreEqual("Adler32", c.Name);
        }
        
        [Test]
        public void Calculate()
        {
            var block = Helpers.TestData;
            
            Assert.AreEqual(2755533412, c.Calculate(block, 0, 100));
            Assert.AreEqual(2888047271, c.Calculate(block, 1, 100));
            Assert.AreEqual(2476743237, c.Calculate(block, 2, 100));
            Assert.AreEqual(591925890, c.Calculate(block, 93, 100));
            Assert.AreEqual(4037189623, c.Calculate(block, 0, block.Length));
        }
        
        [Test]
        public void Rotate()
        {
            Assert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 8));
            Assert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 16));
            Assert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 24));
            Assert.AreEqual(3209698067, c.Rotate(2755533412, 0, 0xAF, 32));
            
            Assert.AreEqual(3577289570, c.Rotate(3209698067, 0xAF, 0xFE, 8));
            Assert.AreEqual(3485539170, c.Rotate(3209698067, 0xAF, 0xFE, 16));
            Assert.AreEqual(3393788770, c.Rotate(3209698067, 0xAF, 0xFE, 24));
            Assert.AreEqual(3302038370, c.Rotate(3209698067, 0xAF, 0xFE, 32));
        }
    }

}