using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Octodiff.Core
{
    public class DeltaBuilder
    {
        public static void BuildDelta(string filePath, List<ChunkSignature> chunks, IDeltaWriter deltaWriter)
        {
            chunks = OrderChunksByChecksum(chunks);

            int minChunkSize;
            int maxChunkSize;
            var chunkMap = CreateChunkMap(chunks, out maxChunkSize, out minChunkSize);

            var buffer = new byte[1 * 1024 * 1024];
            long lastMatchPosition = 0;

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                while (true)
                {
                    var startPosition = stream.Position;
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read < 0)
                        break;

                    var adler = new Adler32RollingChecksum();

                    var remainingPossibleChunkSize = maxChunkSize;

                    for (int i = 0; i < read - minChunkSize; i++)
                    {
                        var readSoFar = startPosition + i;

                        var remainingBytes = buffer.Length - i;
                        if (remainingBytes < maxChunkSize)
                        {
                            remainingPossibleChunkSize = minChunkSize;
                        }

                        if (i == 0)
                        {
                            adler.Initialize(buffer, i, remainingPossibleChunkSize);
                        }
                        else if (remainingBytes < maxChunkSize)
                        {
                            adler.Initialize(buffer, i, remainingPossibleChunkSize);
                        }
                        else
                        {
                            var remove = buffer[i- 1];
                            var add = buffer[i + remainingPossibleChunkSize - 1];
                            adler.Rotate(remove, add, remainingPossibleChunkSize);
                        }

                        if (readSoFar - (lastMatchPosition - remainingPossibleChunkSize) < remainingPossibleChunkSize)
                            continue;

                        if (!chunkMap.ContainsKey(adler.Value)) 
                            continue;
                        
                        var startIndex = chunkMap[adler.Value];

                        for (var j = startIndex; j < chunks.Count && chunks[j].RollingChecksum == adler.Value; j++)
                        {
                            var chunk = chunks[j];
                            
                            var sha = SHA1.Create().ComputeHash(buffer, i, remainingPossibleChunkSize);

                            if (StructuralComparisons.StructuralEqualityComparer.Equals(sha, chunks[j].Hash))
                            {
                                readSoFar = readSoFar + remainingPossibleChunkSize;

                                var missing = readSoFar - lastMatchPosition;
                                if (missing > remainingPossibleChunkSize)
                                {
                                    deltaWriter.WriteDataCommand(stream, lastMatchPosition, missing - remainingPossibleChunkSize);
                                }

                                deltaWriter.WriteCopyCommand(new DataRange(chunk.StartOffset, chunk.Length));
                                lastMatchPosition = readSoFar;
                                break;
                            }
                        }
                    }

                    if (read < buffer.Length)
                    {
                        break;
                    }

                    stream.Position = stream.Position - maxChunkSize + 1;
                }

                if (stream.Length != lastMatchPosition)
                {
                    deltaWriter.WriteDataCommand(stream, lastMatchPosition, stream.Length - lastMatchPosition);
                }
            }
        }

        private static List<ChunkSignature> OrderChunksByChecksum(IEnumerable<ChunkSignature> chunks)
        {
            return chunks.OrderBy(o => o.RollingChecksum).ToList();
        }

        private static Dictionary<uint, int> CreateChunkMap(IList<ChunkSignature> chunks, out int maxChunkSize, out int minChunkSize)
        {
            maxChunkSize = 0;
            minChunkSize = int.MaxValue;

            var chunkMap = new Dictionary<uint, int>();
            for (var i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                if (chunk.Length > maxChunkSize) maxChunkSize = chunk.Length;
                if (chunk.Length < minChunkSize) minChunkSize = chunk.Length;

                if (!chunkMap.ContainsKey(chunk.RollingChecksum))
                {
                    chunkMap[chunk.RollingChecksum] = i;
                }
            }

            return chunkMap;
        }
    }
}