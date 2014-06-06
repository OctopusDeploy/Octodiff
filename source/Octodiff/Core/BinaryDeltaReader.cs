using System;
using System.IO;

namespace Octodiff.Core
{
    public class BinaryDeltaReader : IDeltaReader
    {
        private readonly BinaryReader reader;

        public BinaryDeltaReader(BinaryReader reader)
        {
            this.reader = reader;
        }

        public void Apply(Action<byte[]> writeData, Action<long, long> copy)
        {
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                var b = reader.ReadByte();

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