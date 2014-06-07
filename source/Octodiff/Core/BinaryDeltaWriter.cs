using System;
using System.IO;

namespace Octodiff.Core
{
    public class BinaryDeltaWriter : IDeltaWriter
    {
        private readonly BinaryWriter writer;

        public BinaryDeltaWriter(Stream stream)
        {
            this.writer = new BinaryWriter(stream);
        }

        public void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] basisFileHash)
        {
            writer.Write(8);
            writer.Write("OCTODELTA");
            writer.Write(hashAlgorithm.Name);
            writer.Write(basisFileHash.Length);
            writer.Write(basisFileHash);
            writer.Write(8);
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

                var buffer = new byte[Math.Min((int)length, 1024 * 1024)];

                int read;
                long soFar = 0;
                while ((read = source.Read(buffer, 0, (int)Math.Min(length - soFar, buffer.Length))) > 0)
                {
                    soFar += read;

                    writer.Write(buffer, 0, read);
                }
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