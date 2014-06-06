using System;
using System.IO;

namespace Octodiff.Core
{
    public class DeltaApplier
    {
        public static void ApplyDelta(string baseFile, IDeltaReader delta, string outputFile)
        {
            // ReSharper disable AccessToDisposedClosure
            using (var baseStream = new FileStream(baseFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                delta.Apply(
                    writeData: (data) =>
                    {
                        outputStream.Write(data, 0, data.Length);
                    },
                    copy: (startPosition, length) =>
                    {
                        baseStream.Seek(startPosition, SeekOrigin.Begin);

                        var buffer = new byte[4*1024*1024];
                        int read;
                        long soFar = 0;
                        while ((read = baseStream.Read(buffer, 0, (int)Math.Min(length - soFar, buffer.Length))) > 0)
                        {
                            soFar += read;
                            outputStream.Write(buffer, 0, read);
                        }
                    });
            }
            // ReSharper restore AccessToDisposedClosure
        }
    }
}