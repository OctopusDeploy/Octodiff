using System.Collections.Generic;

namespace Octodiff.Core
{
    class ChunkSignatureChecksumComparer : IComparer<ChunkSignature>
    {
        public int Compare(ChunkSignature x, ChunkSignature y)
        {
            return x.RollingChecksum.CompareTo(y.RollingChecksum);
        }
    }
}