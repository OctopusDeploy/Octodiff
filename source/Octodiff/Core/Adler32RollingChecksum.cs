using System;

namespace Octodiff.Core
{
    public class Adler32RollingChecksum
    {
        private ushort a = 1;
        private ushort b;

        public void Initialize(byte[] block, int offset, int count)
        {
            a = 1;
            b = 0;
            for (var i = offset; i < offset + count; i++)
            {
                var z = block[i];
                a = (ushort)(z + a);
                b = (ushort)(b + a);
            }
        }

        public static UInt32 Calculate(byte[] block, int offset, int count)
        {
            var ad = new Adler32RollingChecksum();
            ad.Initialize(block, offset, count);
            return ad.Value;
        }

        public void Rotate(byte remove, byte add, int chunkSize)
        {
            a = (ushort)((a - remove + add));
            b = (ushort)((b - (chunkSize * remove) + a - 1));
        }

        public UInt32 Value
        {
            get { return (UInt32)((b << 16) | a); }
        }
    }
}