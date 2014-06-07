using System.IO;
using System.Security.Cryptography;

namespace Octodiff.Core
{
    public interface IHashAlgorithm
    {
        string Name { get; }
        int HashLength { get; }
        byte[] ComputeHash(Stream stream);
        byte[] ComputeHash(byte[] buffer, int offset, int length);
    }

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
    }

    public static class SupportedAlgorithms
    {
        public static class Hashing
        {
            public static IHashAlgorithm Sha1()
            {
                return new HashAlgorithmWrapper("SHA1", SHA1.Create());
            }

            public static IHashAlgorithm Default()
            {
                return Sha1();
            }

            public static IHashAlgorithm Create(string algorithm)
            {
                if (algorithm == "SHA1")
                    return Sha1();

                throw new CompatibilityException(string.Format("The hash algorithm '{0}' is not supported in this version of Octodiff", algorithm));
            }
        }

        public static class Checksum
        {
            public static IRollingChecksum Adler32Rolling() { return new Adler32RollingChecksum();  }

            public static IRollingChecksum Default()
            {
                return Adler32Rolling();
            }

            public static IRollingChecksum Create(string algorithm)
            {
                if (algorithm == "Adler32")
                    return Adler32Rolling();
                throw new CompatibilityException(string.Format("The rolling checksum algorithm '{0}' is not supported in this version of Octodiff", algorithm));
            }
        }
    }
}