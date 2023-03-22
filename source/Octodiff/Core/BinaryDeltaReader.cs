using System;
using System.Collections;
using System.IO;
using Octodiff.Diagnostics;

namespace Octodiff.Core
{
    public class BinaryDeltaReader : IDeltaReader
    {
        private readonly BinaryReader reader;
        private readonly IProgressReporter progressReporter;
        private byte[] expectedHash;
        private IHashAlgorithm hashAlgorithm;
        private bool hasReadMetadata;
        private const int DefaultBufferSize = 4 * 1024 * 1024;

        public BinaryDeltaReader(Stream stream, IProgressReporter progressReporter)
        {
            reader = new BinaryReader(stream);
            this.progressReporter = progressReporter ?? NullProgressReporter.Instance;
        }

        public byte[] ExpectedHash
        {
            get
            {
                EnsureMetadata();
                return expectedHash;
            }
        }

        public IHashAlgorithm HashAlgorithm
        {
            get
            {
                EnsureMetadata();
                return hashAlgorithm;
            }
        }

        private void EnsureMetadata()
        {
            if (hasReadMetadata)
                return;

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var first = reader.ReadBytes(BinaryFormat.DeltaHeader.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(first, BinaryFormat.DeltaHeader))
                throw new CorruptFileFormatException("The delta file appears to be corrupt.");

            var version = reader.ReadByte();
            if (version != BinaryFormat.Version)
                throw new CorruptFileFormatException(
                    "The delta file uses a newer file format than this program can handle.");

            var hashAlgorithmName = reader.ReadString();
            hashAlgorithm = SupportedAlgorithms.Hashing.Create(hashAlgorithmName);

            var hashLength = reader.ReadInt32();
            expectedHash = reader.ReadBytes(hashLength);
            var endOfMeta = reader.ReadBytes(BinaryFormat.EndOfMetadata.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.EndOfMetadata, endOfMeta))
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");

            hasReadMetadata = true;
        }

        public void Apply(
            Action<byte[], int, int> writeData,
            Action<long, long> copy)
        {
            EnsureMetadata();

            var fileLength = reader.BaseStream.Length;
            var buffer = new byte[DefaultBufferSize];

            while (reader.BaseStream.Position != fileLength)
            {
                var b = reader.ReadByte();

                progressReporter.ReportProgress("Applying delta", reader.BaseStream.Position, fileLength);
                if (b == BinaryFormat.CopyCommand)
                {
                    var start = reader.ReadInt64();
                    var length = reader.ReadInt64();
                    copy(start, length);
                }
                else if (b == BinaryFormat.DataCommand)
                {
                    var length = reader.ReadInt64();
                    long soFar = 0;
                    while (soFar < length)
                    {
                        var chunkLength = (int)Math.Min(length - soFar, DefaultBufferSize);
                        var numberOfBytesRead = reader.Read(buffer, 0, chunkLength);
                        soFar += numberOfBytesRead;
                        writeData(buffer, 0, numberOfBytesRead);
                    }
                }
            }
        }
    }
}