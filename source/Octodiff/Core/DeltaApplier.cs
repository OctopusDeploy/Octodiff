using System;
using System.Collections;
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
            if (!SkipHashCheck)
            {
                var sourceFileHash = delta.ExpectedHash;
                var algorithm = delta.HashAlgorithm;

                var actualHash = algorithm.ComputeHash(basisFileStream);

                if (!StructuralComparisons.StructuralEqualityComparer.Equals(sourceFileHash, actualHash))
                    throw new UsageException(
                        "The delta file was created from a signature file that does not match the current basis file. It's likely that you are trying to apply this delta to the wrong file, or the file has changed since the signature and delta were created.");

                basisFileStream.Seek(0, SeekOrigin.Begin);
            }

            delta.Apply(
                writeData: (data) => outputStream.Write(data, 0, data.Length),
                copy: (startPosition, length) =>
                {
                    basisFileStream.Seek(startPosition, SeekOrigin.Begin);

                    var buffer = new byte[4*1024*1024];
                    int read;
                    long soFar = 0;
                    while ((read = basisFileStream.Read(buffer, 0, (int)Math.Min(length - soFar, buffer.Length))) > 0)
                    {
                        soFar += read;
                        outputStream.Write(buffer, 0, read);
                    }
                });
        }
    }
}