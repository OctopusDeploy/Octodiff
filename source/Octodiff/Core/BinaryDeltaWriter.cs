using System;
using System.IO;

namespace Octodiff.Core
{
    public class BinaryDeltaWriter : IDeltaWriter
    {
        private readonly BinaryWriter writer;

        public BinaryDeltaWriter(Stream stream)
        {
            writer = new BinaryWriter(stream);
        }

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash)
        {
            writer.Write(BinaryFormat.DeltaHeader);
            writer.Write(BinaryFormat.Version);
            writer.Write(hashAlgorithm.Name);
            writer.Write(expectedNewFileHash.Length);
            writer.Write(expectedNewFileHash);
            writer.Write(BinaryFormat.EndOfMetadata);
        }

        public void WriteCopyCommand(DataRange segment)
        {
            writer.Write(BinaryFormat.CopyCommand);
            writer.Write(segment.StartOffset);
            writer.Write(segment.Length);
        }

        public void WriteDataCommand(Stream source, long offset, long length)
        {
            writer.Write(BinaryFormat.DataCommand);
            writer.Write(length);

            var originalPosition = source.Position;
            try
            {
                source.Seek(offset, SeekOrigin.Begin);

                var buffer = new byte[1024 * 1024];

                int read;
                do
                {
                    read = source.Read(buffer, 0, buffer.Length);

                    writer.Write(buffer, 0, read);
                } while (read > 0);
            }
            finally
            {
                source.Seek(originalPosition, SeekOrigin.Begin);
            }
        }

        public void Finish()
        {
        }
    }
}
