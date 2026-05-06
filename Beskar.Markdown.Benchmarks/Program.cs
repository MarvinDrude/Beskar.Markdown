
using BenchmarkDotNet.Running;
using Beskar.Markdown.Benchmarks.Comparisons;
using Beskar.Markdown.Rendering;

#if !DEBUG 

BenchmarkRunner.Run<MarkdigBenchmark>();

#else

Console.WriteLine("Debug mode");
var ben = new MarkdigBenchmark();
ben.Setup();

var ma1 = ben.SimpleMarkdig();
var ma2 = ben.FullSpecMarkdig();
var ma3 = ben.BigMarkdig();

//var be1 = ben.SimpleBeMarkdown();
var be2 = ben.FullSpecBeMarkdown();
//var be3 = ben.BigBeMarkdown();

_ = "";

#endif
