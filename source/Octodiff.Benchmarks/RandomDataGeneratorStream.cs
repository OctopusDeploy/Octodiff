namespace Octodiff.Benchmarks;

public class RandomDataGeneratorStream : Stream
{
    private readonly byte[] masterBuffer = new byte[128];
    private long position;

    public RandomDataGeneratorStream(long desiredLength, int? seed = null)
    {
        Length = desiredLength;
        var random = new Random(seed ?? (int)DateTime.Now.Ticks);
        random.NextBytes(masterBuffer);
    }

    public override void Flush()
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var toCopy = Math.Min(count, Length - position);
        var copied = 0;
        while (copied < toCopy)
        {
            var startingPos = (int)(position % masterBuffer.Length);
            var copyThisTime = (int)Math.Min(masterBuffer.Length - startingPos, toCopy - copied);
            Buffer.BlockCopy(masterBuffer, startingPos, buffer, offset, copyThisTime);
            offset += copyThisTime;
            position += copyThisTime;
            copied += copyThisTime;
        }
        return (int)toCopy;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => position + offset,
            SeekOrigin.End => Length - offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
        };
        return position;
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length { get; }

    public override long Position
    {
        get => position;
        set => position = value;
    }
}