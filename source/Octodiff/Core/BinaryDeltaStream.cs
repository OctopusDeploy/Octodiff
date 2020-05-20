using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Octodiff.Diagnostics;

namespace Octodiff.Core
{

    public class BinaryDeltaStream : Stream, IDeltaStream
    {
        class CommandData
        {
            public bool IsOrigin { get; set; }
            public long CommandStartLocation { get; set; }
            public long DestinationFileLocation { get; set; }
            public long SrcLocation { get; set; }
            public long Length { get; set; }
        }

        private readonly BinaryReader reader;
        private byte[] expectedHash;
        private IHashAlgorithm hashAlgorithm;
        private bool hasReadMetadata;
        SortedList<long, CommandData> commands = new SortedList<long, CommandData>();

        private long _length;
        public override long Length { get { EnsureMetadata(); return _length; } }

        private Func<byte[], long, int, int?, int> OriginRead { get; }
        private Func<byte[], long, int, int?, int> DeltaRead { get; }

        public BinaryDeltaStream(Stream basisStream, Stream delta)
        {
            this.reader = new BinaryReader(delta);

            OriginRead = (byte[] buffer, long startBytes, int offset, int? count) =>
            {
                basisStream.Seek(startBytes, SeekOrigin.Begin);
                return basisStream.Read(buffer, offset, count ?? buffer.Length);
            };
            DeltaRead = (byte[] buffer, long startBytes, int offset, int? count) =>
            {
                reader.BaseStream.Seek(startBytes, SeekOrigin.Begin);
                return reader.BaseStream.Read(buffer, offset, count ?? buffer.Length);
            };
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

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Position { get; set; } = 0;

        void EnsureMetadata()
        {
            if (hasReadMetadata)
                return;

            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var first = reader.ReadBytes(BinaryFormat.DeltaHeader.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(first, BinaryFormat.DeltaHeader))
                throw new CorruptFileFormatException("The delta file appears to be corrupt.");

            var version = reader.ReadByte();
            if (version != BinaryFormat.Version)
                throw new CorruptFileFormatException("The delta file uses a newer file format than this program can handle.");

            var hashAlgorithmName = reader.ReadString();
            hashAlgorithm = SupportedAlgorithms.Hashing.Create(hashAlgorithmName);

            var hashLength = reader.ReadInt32();
            expectedHash = reader.ReadBytes(hashLength);
            var endOfMeta = reader.ReadBytes(BinaryFormat.EndOfMetadata.Length);
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(BinaryFormat.EndOfMetadata, endOfMeta))
                throw new CorruptFileFormatException("The signature file appears to be corrupt.");


            var fileLength = reader.BaseStream.Length;
            //long currentLocation = 0;
            long outputLocation = 0;

            while (reader.BaseStream.Position != fileLength)
            {
                var b = reader.ReadByte();

                if (b == BinaryFormat.CopyCommand)
                {
                    var start = reader.ReadInt64();
                    var length = reader.ReadInt64();
                    commands.Add(outputLocation, new CommandData
                    {
                        IsOrigin = true,
                        DestinationFileLocation = outputLocation,
                        Length = length,
                        CommandStartLocation = reader.BaseStream.Position - (sizeof(long) * 2),
                        SrcLocation = start
                    });
                    outputLocation += length;
                }
                else if (b == BinaryFormat.DataCommand)
                {
                    var length = reader.ReadInt64();
                    commands.Add(outputLocation, new CommandData
                    {
                        IsOrigin = false,
                        DestinationFileLocation = outputLocation,
                        Length = length,
                        CommandStartLocation = reader.BaseStream.Position - (sizeof(long) * 2),
                        SrcLocation = reader.BaseStream.Position
                    });
                    reader.BaseStream.Seek(length, SeekOrigin.Current);
                    outputLocation += length;

                }
            }
            _length = commands.Last().Key + commands.Last().Value.Length;

            hasReadMetadata = true;
        }

        public int ReadAt(byte[] buffer, long startBytes, int offset = 0, int? count = null)
        {
            var localCount = count == null ? buffer.Length - offset : Math.Min(buffer.Length - offset, count.Value);
            var fileLength = reader.BaseStream.Length;

            EnsureMetadata();
            int currentBytes = 0;

            while ((startBytes + currentBytes) < Length && localCount > currentBytes)
            {
                var currentStart = startBytes + currentBytes;
                var nextCmd = commands.Where(s => s.Key <= currentStart).Last().Value;

                var start = nextCmd.SrcLocation + (currentStart - nextCmd.DestinationFileLocation);
                var length = nextCmd.Length - (currentStart - nextCmd.DestinationFileLocation);

                int read;
                long soFar = 0;
                while ((read = (nextCmd.IsOrigin ? OriginRead : DeltaRead).Invoke(buffer, start, currentBytes + offset, (int)Math.Min(length - soFar, localCount - currentBytes))) > 0)
                {
                    soFar += read;
                    currentBytes += read;
                }

            }
            return currentBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long targetPostion;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    targetPostion = offset;
                    break;
                case SeekOrigin.Current:
                    targetPostion = Position + offset;
                    break;
                case SeekOrigin.End:
                    targetPostion = Length + offset;
                    break;
                default:
                    throw new ArgumentException("Invalid SeekOrigin");
            }

            if (targetPostion < 0)
                throw new IOException("The resulting position must be greater than zero.");
            if (Length < targetPostion)
                throw new IOException("The resulting position must be less than the length");

            Position = targetPostion;
            return Position;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if(Position >= Length - 1)
            {
                return 0;
            }
            int move = ReadAt(buffer, Position, offset, count);
            Position += move;
            return move;
        }

        public bool VerifyHashInMemory()
        {
            Seek(0L, SeekOrigin.Begin);
            var actualHash = HashAlgorithm.ComputeHash(this);

            return StructuralComparisons.StructuralEqualityComparer.Equals(ExpectedHash, actualHash);
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}