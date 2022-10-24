using System;
using System.Collections;
using System.IO;

namespace Octodiff.Core
{
    public class DeltaApplier
    {
        private const int BufferLength = 4 * 1024 * 1024;

        public DeltaApplier()
        {
            SkipHashCheck = false;
        }

        public bool SkipHashCheck { get; set; }

        public void Apply(Stream basisFileStream, IDeltaReader delta, Stream outputStream)
        {
            var buffer = new byte[BufferLength];

            delta.Apply(
                writeData: outputStream.Write,
                copy: (offset, count) =>
                {
                    basisFileStream.Seek(offset, SeekOrigin.Begin);

                    int read;
                    long soFar = 0;
                    while ((read = basisFileStream.Read(buffer, 0, (int)Math.Min(count - soFar, BufferLength))) > 0)
                    {
                        soFar += read;
                        outputStream.Write(buffer, 0, read);
                    }
                });

            if (SkipHashCheck)
                return;

            outputStream.Seek(0, SeekOrigin.Begin);

            var sourceFileHash = delta.ExpectedHash;
            var algorithm = delta.HashAlgorithm;

            var actualHash = algorithm.ComputeHash(outputStream);

            if (!StructuralComparisons.StructuralEqualityComparer.Equals(sourceFileHash, actualHash))
                throw new UsageException(
                    "Verification of the patched file failed. The SHA1 hash of the patch result file, and the file that was used as input for the delta, do not match. This can happen if the basis file changed since the signatures were calculated.");
        }
    }
}