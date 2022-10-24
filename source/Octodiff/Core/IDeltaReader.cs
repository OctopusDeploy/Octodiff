using System;

namespace Octodiff.Core
{
    public interface IDeltaReader
    {
        byte[] ExpectedHash { get; }
        IHashAlgorithm HashAlgorithm { get; }

        /// <summary>
        /// Reads the delta file.
        /// This method will invoke the Action to copy the data from the original file multiple times if needed.
        /// This method will also invoke the Action to write data from the delta file to the destination file multiple times if needed.
        /// </summary>
        /// <param name="writeData">Action to write data from the delta file to the destination file. Parameters: buffer, offset and count.</param>
        /// <param name="copy">Action to copy data from the original file to the destination file. Parameters: offset and count.</param>
        void Apply(
            Action<byte[], int, int> writeData,
            Action<long, long> copy
            );
    }
}