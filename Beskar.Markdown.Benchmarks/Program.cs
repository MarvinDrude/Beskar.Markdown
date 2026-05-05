
using BenchmarkDotNet.Running;
using Beskar.Markdown.Benchmarks.Comparisons;

#if !DEBUG 

BenchmarkRunner.Run<MarkdigBenchmark>();

#else

Console.WriteLine("Debug mode");

#endif
