using BenchmarkDotNet.Running;

namespace Benchmark; 
internal class Program {
    static void Main() {
        BenchmarkRunner.Run<BenchmarkDemo>();

    }
}