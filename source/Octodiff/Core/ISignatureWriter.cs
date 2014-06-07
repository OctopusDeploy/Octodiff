using System.IO;

namespace Octodiff.Core
{
    public interface ISignatureWriter
    {
        void WriteSettings(string hashAlgorithm, string rollingChecksumAlgorithm, short chunkSize);
        void WriteBasisFileHash(byte[] hash);
        void WriteChunk(ChunkSignature signature);
    }

    class SignatureWriter
        : ISignatureWriter
    {
        private readonly BinaryWriter signatureStream;

        public SignatureWriter(Stream signatureStream)
        {
            this.signatureStream = new BinaryWriter(signatureStream);
        }

        public void WriteSettings(string hashAlgorithm, string rollingChecksumAlgorithm, short chunkSize)
        {
            signatureStream.Write(hashAlgorithm);
            signatureStream.Write(rollingChecksumAlgorithm);
            signatureStream.Write(chunkSize);
        }

        public void WriteBasisFileHash(byte[] hash)
        {
            signatureStream.Write(hash.Length);
            signatureStream.Write(hash);
        }

        public void WriteChunk(ChunkSignature signature)
        {
            signatureStream.Write(signature.Length);
            signatureStream.Write(signature.RollingChecksum);
            signatureStream.Write(signature.Hash);
        }
    }
}