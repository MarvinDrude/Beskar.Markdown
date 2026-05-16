using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Beskar.Markdown.Benchmarks.Config;

public class WarmupDetailedConfig : DetailedConfig
{
   public WarmupDetailedConfig()
   {
      AddJob(Job.Default
         .WithWarmupCount(1)
         .WithIterationCount(4)
         .WithInvocationCount(16 * 2)
         .WithStrategy(RunStrategy.Throughput));
   }
}