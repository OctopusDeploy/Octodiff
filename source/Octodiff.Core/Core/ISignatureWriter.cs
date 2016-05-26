using System.IO;

namespace Octodiff.Core
{
    public interface ISignatureWriter
    {
        void WriteMetadata(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm, byte[] hash);
        void WriteChunk(ChunkSignature signature);
    }
}