using System.Security.Cryptography;

namespace Octodiff.Core
{
    public interface IHashingSupportedAlgorithm
    {
        IHashAlgorithm Default();
        IHashAlgorithm Create(string algorithm);
    }

    public class DefaultHashingSupportedAlgorithm
        : IHashingSupportedAlgorithm
    {
        public IHashAlgorithm Sha1()
        {
            return new HashAlgorithmWrapper("SHA1", SHA1.Create());
        }

        public virtual IHashAlgorithm Default()
        {
            return Sha1();
        }

        public virtual IHashAlgorithm Create(string algorithm)
        {
            if (algorithm == "SHA1")
                return Sha1();

            throw new CompatibilityException(
                $"The hash algorithm '{algorithm}' is not supported in this version of Octodiff");
        }
    }

    public interface IChecksumSupportedAlgorithm
    {
        IRollingChecksum Default();
        IRollingChecksum Create(string algorithm);
    }

    public class DefaultChecksumSupportedAlgorithm
        : IChecksumSupportedAlgorithm
    {
#pragma warning disable 618
        public IRollingChecksum Adler32Rolling(bool useV2 = false)
        {
            if (useV2)
                return new Adler32RollingChecksumV2();

            return new Adler32RollingChecksum();
        }
#pragma warning restore 618

        public virtual IRollingChecksum Default()
        {
            return Adler32Rolling();
        }

        public virtual IRollingChecksum Create(string algorithm)
        {
            switch (algorithm)
            {
                case "Adler32":
                    return Adler32Rolling();
                case "Adler32V2":
                    return Adler32Rolling(true);
            }
            throw new CompatibilityException(
                $"The rolling checksum algorithm '{algorithm}' is not supported in this version of Octodiff");
        }
    }

    public static class SupportedAlgorithms
    {
        public static IHashingSupportedAlgorithm Hashing { get; set; } = new DefaultHashingSupportedAlgorithm();

        public static IChecksumSupportedAlgorithm Checksum { get; set; } = new DefaultChecksumSupportedAlgorithm();
    }
}