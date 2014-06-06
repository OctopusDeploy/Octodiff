using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Octodiff.Core
{
    public class SignatureBuilder
    {
        public static List<ChunkSignature> BuildSignature(string filePath, int blockSize)
        {
            var block = new byte[blockSize];

            var chunks = new List<ChunkSignature>();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                while (true)
                {
                    var start = stream.Position;
                    var read = stream.Read(block, 0, blockSize);
                    if (read <= 0)
                        break;

                    chunks.Add(new ChunkSignature
                    {
                        StartOffset = start,
                        Length = read,
                        Hash = SHA1.Create().ComputeHash(block, 0, read),
                        RollingChecksum = Adler32RollingChecksum.Calculate(block, 0, read)
                    });
                }
            }

            return chunks;
        }
    }
}