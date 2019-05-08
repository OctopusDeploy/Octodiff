using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Octodiff.Diagnostics;

namespace Octodiff.Core
{
    public class DeltaApplier
    {
        public DeltaApplier()
        {
            SkipHashCheck = false;
        }

        public bool SkipHashCheck { get; set; }

        public void Apply(Stream basisFileStream, IDeltaReader delta, Stream outputStream)
        {
            delta.Apply(basisFileStream, outputStream, SkipHashCheck);
        }

        public void Apply(IDeltaStream delta, Stream outputStream)
        {
            delta.Apply(outputStream, SkipHashCheck);
        }
    }

    public static class DeltaApplierExtensionMethods
    {
        public static void Apply(this IDeltaStream delta, Stream outputStream, bool SkipHashCheck = false)
        {
            var buffer = new byte[4 * 1024 * 1024];
            var offset = 0L;
            var currentSize = 0;
            while ((currentSize = delta.ReadAt(buffer, offset)) > 0)
            {
                outputStream.Write(buffer, 0, currentSize);
                offset += currentSize;
            }

            if (!SkipHashCheck)
            {
                outputStream.Flush();
                if (!delta.IsHashValid(outputStream))
                {
                    throw new UsageException("Verification of the patched file failed. The SHA1 hash of the patch result file, and the file that was used as input for the delta, do not match. This can happen if the basis file changed since the signatures were calculated.");
                }

            }
        }

        public static bool IsHashValid(this IDeltaStream delta, Stream outputStream)
        {
            outputStream.Seek(0, SeekOrigin.Begin);

            var sourceFileHash = delta.ExpectedHash;
            var algorithm = delta.HashAlgorithm;

            var actualHash = algorithm.ComputeHash(outputStream);

            return StructuralComparisons.StructuralEqualityComparer.Equals(sourceFileHash, actualHash);
        }

        public static void Apply(this IDeltaReader delta, Stream basisFileStream, Stream outputStream, bool SkipHashCheck = false)
        {
            delta.Apply(
                writeData: (data) => outputStream.Write(data, 0, data.Length),
                copy: (startPosition, length) =>
                {
                    basisFileStream.Seek(startPosition, SeekOrigin.Begin);

                    var buffer = new byte[4 * 1024 * 1024];
                    int read;
                    long soFar = 0;
                    while ((read = basisFileStream.Read(buffer, 0, (int)Math.Min(length - soFar, buffer.Length))) > 0)
                    {
                        soFar += read;
                        outputStream.Write(buffer, 0, read);
                    }
                });

            if (!SkipHashCheck)
            {
                if (!delta.IsHashValid(outputStream))
                {
                    throw new UsageException("Verification of the patched file failed. The SHA1 hash of the patch result file, and the file that was used as input for the delta, do not match. This can happen if the basis file changed since the signatures were calculated.");
                }
            }
        }

        public static bool IsHashValid(this IDeltaReader delta, Stream outputStream)
        {
            outputStream.Seek(0, SeekOrigin.Begin);

            var sourceFileHash = delta.ExpectedHash;
            var algorithm = delta.HashAlgorithm;

            var actualHash = algorithm.ComputeHash(outputStream);

            return StructuralComparisons.StructuralEqualityComparer.Equals(sourceFileHash, actualHash);
        }
    }
}