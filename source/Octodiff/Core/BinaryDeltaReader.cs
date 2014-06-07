using System;
using System.IO;
using System.Security.Policy;
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

        public BinaryDeltaReader(Stream stream, IProgressReporter progressReporter)
        {
            this.reader = new BinaryReader(stream);
            this.progressReporter = progressReporter ?? new NullProgressReporter();
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

        void EnsureMetadata()
        {
            if (hasReadMetadata)
                return;

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
        
            var first = reader.ReadInt32();
            if (first != 8)
                throw new CorruptFileFormatException("The delta file appears to be corrupt");

            var message = reader.ReadString();
            if (message != "OCTODELTA")
                throw new CorruptFileFormatException("The delta file appears to be corrupt");

            var hashAlgorithmName = reader.ReadString();
            hashAlgorithm = SupportedAlgorithms.Hashing.Create(hashAlgorithmName);

            var hashLength = reader.ReadInt32();
            expectedHash = reader.ReadBytes(hashLength);
            first = reader.ReadInt32();
            if (first != 8)
                throw new CorruptFileFormatException("The delta file appears to be corrupt");

            hasReadMetadata = true;
        }

        public void Apply(
            Action<byte[]> writeData, 
            Action<long, long> copy)
        {
            var fileLength = reader.BaseStream.Length;

            EnsureMetadata();

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
                        var bytes = reader.ReadBytes((int) Math.Min(length - soFar, 1024*1024*4));
                        soFar += bytes.Length;
                        writeData(bytes);
                    }
                }
            }
        }
    }
}