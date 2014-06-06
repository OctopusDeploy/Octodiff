using System;

namespace Octodiff.Core
{
    public class ChunkSignature
    {
        public long StartOffset;            // 8
        public short Length;                // 2
        public byte[] Hash;                 // 20
        public UInt32 RollingChecksum;      // 4
                                            // 34 bytes

        public override string ToString()
        {
            return string.Format("{0,6}:{1,6} |{2,20}| {3}", StartOffset, Length, RollingChecksum, BitConverter.ToString(Hash).ToLowerInvariant().Replace("-", ""));
        }
    }
}