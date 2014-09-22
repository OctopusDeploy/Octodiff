using System.IO;
using Octodiff.Diagnostics;

namespace Octodiff.Core
{
    public class SignatureBuilder
    {
        public static readonly short MinimumChunkSize = 128;
        public static readonly short DefaultChunkSize = 2048;
        public static readonly short MaximumChunkSize = 31 * 1024;

        private short chunkSize;

        public SignatureBuilder()
        {
            ChunkSize = DefaultChunkSize;
            HashAlgorithm = SupportedAlgorithms.Hashing.Default();
            RollingChecksumAlgorithm = SupportedAlgorithms.Checksum.Default();
            ProgressReporter = new NullProgressReporter();
        }

        public IProgressReporter ProgressReporter { get; set; }

        public IHashAlgorithm HashAlgorithm { get; set; }

        public IRollingChecksum RollingChecksumAlgorithm { get; set; }

        public short ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value < MinimumChunkSize)
                    throw new UsageException(string.Format("Chunk size cannot be less than {0}", MinimumChunkSize));
                if (value > MaximumChunkSize)
                    throw new UsageException(string.Format("Chunk size cannot be exceed {0}", MaximumChunkSize));
                chunkSize = value;
            }
        }

        public void Build(Stream stream, ISignatureWriter signatureWriter)
        {
            signatureWriter.WriteBegin(HashAlgorithm, RollingChecksumAlgorithm);
            byte[] hash;
            WriteChunkSignatures(stream, signatureWriter, out hash);
            WriteMetadata(stream, signatureWriter, hash);
        }

        void WriteMetadata(Stream stream, ISignatureWriter signatureWriter, byte[] hash)
        {
            ProgressReporter.ReportProgress("Hashing file", 0, stream.Length);
            stream.Seek(0, SeekOrigin.Begin);

            signatureWriter.WriteMetadata(HashAlgorithm, RollingChecksumAlgorithm, hash);

            ProgressReporter.ReportProgress("Hashing file", stream.Length, stream.Length);
        }

        void WriteChunkSignatures(Stream stream, ISignatureWriter signatureWriter, out byte[] hash)
        {
            var checksumAlgorithm = RollingChecksumAlgorithm;
            var hashAlgorithm = HashAlgorithm;
            var fullFileChecksumAlgorithm = (IHashAlgorithm)HashAlgorithm.Clone();

            ProgressReporter.ReportProgress("Building signatures", 0, stream.Length);
            stream.Seek(0, SeekOrigin.Begin);

            long start = 0;
            int read;
            var block = new byte[ChunkSize];
            while ((read = stream.Read(block, 0, block.Length)) > 0)
            {
                signatureWriter.WriteChunk(new ChunkSignature
                {
                    StartOffset = start,
                    Length = (short)read,
                    Hash = hashAlgorithm.ComputeHash(block, 0, read),
                    RollingChecksum = checksumAlgorithm.Calculate(block, 0, read)
                });

                fullFileChecksumAlgorithm.TransformBlock(block, 0, read);

                start += read;
                ProgressReporter.ReportProgress("Building signatures", start, stream.Length);
            }

            hash = fullFileChecksumAlgorithm.TransformFinal();
        }
    }
}