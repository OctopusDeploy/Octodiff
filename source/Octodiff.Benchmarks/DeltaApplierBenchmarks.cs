using BenchmarkDotNet.Attributes;
using Octodiff.Core;
using Octodiff.Diagnostics;

namespace Octodiff.Benchmarks;

[MemoryDiagnoser]
public class DeltaApplierBenchmarks
{
    private const int _500MB = 0x320_0000;
    private Stream? deltaStream;
    private Stream? otherDeltaStream;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var originalStream = new RandomDataGeneratorStream(_500MB, 100);
        var newStream = new RandomDataGeneratorStream(_500MB, 200);
        var signatureStream = new MemoryStream();
        var signatureBuilder = new SignatureBuilder();
        signatureBuilder.Build(originalStream, new SignatureWriter(signatureStream));

        signatureStream.Seek(0, SeekOrigin.Begin);
        var deltaBuilder = new DeltaBuilder();
        deltaStream = new MemoryStream();
        deltaBuilder.BuildDelta(newStream, new SignatureReader(signatureStream, new NullProgressReporter()), new BinaryDeltaWriter(deltaStream));
        deltaStream.Seek(0, SeekOrigin.Begin);

        originalStream.Seek(0, SeekOrigin.Begin);
        signatureStream.Seek(0, SeekOrigin.Begin);
        otherDeltaStream = new MemoryStream();
        deltaBuilder.BuildDelta(originalStream, new SignatureReader(signatureStream, new NullProgressReporter()), new BinaryDeltaWriter(otherDeltaStream));
        otherDeltaStream.Seek(0, SeekOrigin.Begin);
    }

    [Benchmark]
    public void ApplyBigDelta_Different()
    {
        var originalStream = new RandomDataGeneratorStream(_500MB, 100);
        var deltaApplier = new DeltaApplier { SkipHashCheck = true };
        deltaApplier.Apply(originalStream, new BinaryDeltaReader(deltaStream, new NullProgressReporter()), Stream.Null);
    }

    [Benchmark]
    public void ApplyBigDelta_Identical()
    {
        var originalStream = new RandomDataGeneratorStream(_500MB, 100);
        var deltaApplier = new DeltaApplier { SkipHashCheck = true };
        deltaApplier.Apply(originalStream, new BinaryDeltaReader(otherDeltaStream, new NullProgressReporter()), Stream.Null);
    }
}