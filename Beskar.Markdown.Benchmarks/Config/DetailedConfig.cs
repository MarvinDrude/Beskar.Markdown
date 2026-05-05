using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;

namespace Beskar.Markdown.Benchmarks.Config;

public class DetailedConfig : ManualConfig
{
   public DetailedConfig()
   {
      AddLogger(ConsoleLogger.Default);

      AddColumn(StatisticColumn.AllStatistics); 
      AddColumn(RankColumn.Arabic);
      AddColumn(BaselineRatioColumn.RatioMean);

      AddDiagnoser(MemoryDiagnoser.Default);
      AddDiagnoser(ThreadingDiagnoser.Default);

      AddExporter(MarkdownExporter.GitHub);
      AddExporter(HtmlExporter.Default);
      AddExporter(CsvExporter.Default);
      AddExporter(JsonExporter.Full);

      SummaryStyle = SummaryStyle.Default
         .WithRatioStyle(RatioStyle.Trend);
   }
}