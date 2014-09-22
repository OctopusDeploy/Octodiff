using System.IO;

namespace Octodiff.Core
{
    public interface IDeltaWriter
    {
        void Begin(IHashAlgorithm hashAlgorithm);
        void WriteMetadata(IHashAlgorithm hashAlgorithm, byte[] expectedNewFileHash);
        void WriteCopyCommand(DataRange segment);
        void WriteDataCommand(Stream source, long offset, long length);
        void FinishCommands();
    }
}