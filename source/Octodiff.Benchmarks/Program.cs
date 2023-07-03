using BenchmarkDotNet.Running;

namespace Octodiff.Benchmarks;

static class Program
{
    public static void Main(string[] args) => BenchmarkRunner.Run<DeltaApplierBenchmarks>(null, args);        
}