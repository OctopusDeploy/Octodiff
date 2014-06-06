using System;
using System.IO;

namespace Octodiff.Core
{
    public class BinaryDeltaWriter : IDeltaWriter
    {
        private readonly BinaryWriter writer;

        public BinaryDeltaWriter(BinaryWriter writer)
        {
            this.writer = writer;
        }

        public void WriteMetadata()
        {
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