using System.IO;
using System.Security.Cryptography;

namespace Octodiff.Core
{
    public class HashAlgorithmWrapper : IHashAlgorithm
    {
        private readonly HashAlgorithm algorithm;

        public HashAlgorithmWrapper(string name, HashAlgorithm algorithm)
        {
            Name = name;
            this.algorithm = algorithm;
        }

        public string Name { get; private set; }
        public int HashLength { get { return algorithm.HashSize / 8; } }

        public byte[] ComputeHash(Stream stream)
        {
            return algorithm.ComputeHash(stream);
        }

        public byte[] ComputeHash(byte[] buffer, int offset, int length)
        {
            return algorithm.ComputeHash(buffer, offset, length);
        }

        public void TransformBlock(byte[] buffer, int length)
        {
            algorithm.TransformBlock(buffer, 0, length, buffer, 0);
        }

        public byte[] TransformFinal()
        {
            algorithm.TransformFinalBlock(new byte[0], 0, 0);
            return algorithm.Hash;
        }

        public object Clone()
        {
            return SupportedAlgorithms.Hashing.Create(Name);
        }
    }
}