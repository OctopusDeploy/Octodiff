using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Octodiff.Diagnostics;

namespace Octodiff.Core
{
    public class SignatureReader : ISignatureReader
    {
        private readonly IProgressReporter reporter;
        private readonly BinaryReader reader;

        public SignatureReader(Stream stream, IProgressReporter reporter)
        {
            this.reporter = reporter;
            this.reader = new BinaryReader(stream);
        }

        public Signature ReadSignature()
        {
            Progress();
            var header = reader.ReadInt32();
            if (header != 8) 
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            var hashAlgorithm = reader.ReadString();
            var rollingChecksumAlgorithm = reader.ReadString();
            var hashLength = reader.ReadInt32();
            var hash = reader.ReadBytes(hashLength);
            
            header = reader.ReadInt32();
            if (header != 8)
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            Progress();

            var hashAlgo = SupportedAlgorithms.Hashing.Create(hashAlgorithm);
            var signature = new Signature(
                hashAlgo,
                SupportedAlgorithms.Checksum.Create(rollingChecksumAlgorithm),
                hash);

            var expectedHashLength = hashAlgo.HashLength;
            long start = 0;

            var fileLength = reader.BaseStream.Length;
            var remainingBytes = fileLength - reader.BaseStream.Position;
            var signatureSize = sizeof (ushort) + sizeof (uint) + expectedHashLength;
            if (remainingBytes % signatureSize != 0)
                throw new CorruptFileFormatException("The signature file appears to be corrupt; at least one chunk has data missing.");

            while (reader.BaseStream.Position < fileLength - 1)
            {
                var length = reader.ReadInt16();
                var checksum = reader.ReadUInt32();
                var chunkHash = reader.ReadBytes(expectedHashLength);

                signature.Chunks.Add(new ChunkSignature
                {
                    StartOffset = start,
                    Length = length,
                    RollingChecksum = checksum,
                    Hash = chunkHash
                });

                start += length;

                Progress();
            }

            return signature;
        }

        void Progress()
        {
            reporter.ReportProgress("Reading signature", reader.BaseStream.Position, reader.BaseStream.Length);
        }
    }
}