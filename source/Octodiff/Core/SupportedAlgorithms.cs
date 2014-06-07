using System.Security.Cryptography;

namespace Octodiff.Core
{
    public static class SupportedAlgorithms
    {
        public static class Hashing
        {
            public static HashAlgorithm Sha1()
            {
                return SHA1.Create();
            }

            public static HashAlgorithm Default()
            {
                return Sha1();
            }

            public static HashAlgorithm Create(string algorithm)
            {
                if (algorithm == "SHA1")
                    return Sha1();
                throw new CompatibilityException("The hash algorithm '{0}' is not supported in this version of Octodiff");
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
                if (algorithm == "Adler32Rolling")
                    return Adler32Rolling();
                throw new CompatibilityException("The rolling checksum algorithm '{0}' is not supported in this version of Octodiff");
            }
        }
    }
}