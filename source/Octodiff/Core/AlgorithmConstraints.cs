namespace Octodiff.Core
{
    public static class AlgorithmConstraints
    {
        public static readonly ushort MinimumChunkSignatureLength = 128;
        public static readonly ushort DefaultChunkSignatureLength = 2048;
        public static readonly ushort MaximumChunkSignatureLength = 31 * 1024;
        public static readonly ushort ChunkSignatureSize = 34;
    }
}