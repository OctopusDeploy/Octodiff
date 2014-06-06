using System;

namespace Octodiff.Core
{
    public interface IDeltaReader
    {
        void Apply(
            Action<byte[]> writeData,
            Action<long, long> copy
            );
    }
}