using System;
using System.IO;

namespace Octodiff.Core
{
    public interface IHashAlgorithm : ICloneable
    {
        string Name { get; }
        int HashLength { get; }
        byte[] ComputeHash(Stream stream);
        byte[] ComputeHash(byte[] buffer, int offset, int length);
        void TransformBlock(byte[] buffer, int offset, int length);
        byte[] TransformFinal();
    }
}