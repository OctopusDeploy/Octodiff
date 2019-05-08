namespace Octodiff.Core
{
    public interface IDeltaStream
    {
        byte[] ExpectedHash { get; }
        IHashAlgorithm HashAlgorithm { get; }
        long Length { get; }

        int ReadAt(byte[] buffer, long startBytes, int offset = 0, int? count = null);
    }
}