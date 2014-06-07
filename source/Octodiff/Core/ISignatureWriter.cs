using System.IO;

namespace Octodiff.Core
{
    public interface ISignatureWriter
    {
        void WriteMetadata(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm, byte[] hash);
        void WriteChunk(ChunkSignature signature);
    }

    class SignatureWriter : ISignatureWriter
    {
        private readonly BinaryWriter signatureStream;

        public SignatureWriter(Stream signatureStream)
        {
            this.signatureStream = new BinaryWriter(signatureStream);
        }

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, IRollingChecksum rollingChecksumAlgorithm, byte[] hash)
        {
            signatureStream.Write(8);
            signatureStream.Write(hashAlgorithm.Name);
            signatureStream.Write(rollingChecksumAlgorithm.Name);
            signatureStream.Write(hash.Length);
            signatureStream.Write(hash);
            signatureStream.Write(8);
        }

        public void WriteChunk(ChunkSignature signature)
        {
            signatureStream.Write(signature.Length);
            signatureStream.Write(signature.RollingChecksum);
            signatureStream.Write(signature.Hash);
        }
    }
}