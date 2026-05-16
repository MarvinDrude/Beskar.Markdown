using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Beskar.Markdown.Benchmarks.Config;

namespace Beskar.Markdown.Benchmarks.Comparisons;

[Config(typeof(WarmupDetailedConfig))]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ComparisonBenchmark
{
   private string _markdown = string.Empty;
   private string _test = string.Empty;
   private string _test2 = string.Empty;
   
   [GlobalSetup]
   public void Setup()
   {
      _markdown = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "spec.md"));
      _test = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "test.md"));
      _test2 = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "test2.md"));
   }
   
   [Benchmark(Description = "Markdig")]
   [BenchmarkCategory("Test 1")]
   public string Test1Markdig()
   {
      return Markdig.Markdown.ToHtml(_test);
   }

   [Benchmark(Description = "Beskar.Markdown")]
   [BenchmarkCategory("Test 1")]
   public string Test1BeMarkdown()
   {
      return BeMarkdown.ToHtml(_test);
   }

   [Benchmark(Description = "CommonMark.Net")]
   [BenchmarkCategory("Test 1")]
   public string Test1CommonMarkNet()
   {
      return CommonMark.CommonMarkConverter.Convert(_test);
   }

   [Benchmark(Description = "MarkdownSharp")]
   [BenchmarkCategory("Test 1")]
   public string Test1MarkdownSharp()
   {
      return new MarkdownSharp.Markdown().Transform(_test);
   }

   [Benchmark(Description = "Markdig")]
   [BenchmarkCategory("Test 2")]
   public string Test2Markdig()
   {
      return Markdig.Markdown.ToHtml(_test2);
   }

   [Benchmark(Description = "Beskar.Markdown")]
   [BenchmarkCategory("Test 2")]
   public string Test2BeMarkdown()
   {
      return BeMarkdown.ToHtml(_test2);
   }

   [Benchmark(Description = "CommonMark.Net")]
   [BenchmarkCategory("Test 2")]
   public string Test2CommonMarkNet()
   {
      return CommonMark.CommonMarkConverter.Convert(_test2);
   }

   [Benchmark(Description = "MarkdownSharp")]
   [BenchmarkCategory("Test 2")]
   public string Test2MarkdownSharp()
   {
      return new MarkdownSharp.Markdown().Transform(_test2);
   }
   
   [Benchmark(Description = "Markdig")]
   [BenchmarkCategory("Full Spec")]
   public string FullSpecMarkdig()
   {
      return Markdig.Markdown.ToHtml(_markdown);
   }

   [Benchmark(Description = "Beskar.Markdown")]
   [BenchmarkCategory("Full Spec")]
   public string FullSpecBeMarkdown()
   {
      return BeMarkdown.ToHtml(_markdown);
   }

   [Benchmark(Description = "CommonMark.Net")]
   [BenchmarkCategory("Full Spec")]
   public string FullSpecCommonMarkNet()
   {
      return CommonMark.CommonMarkConverter.Convert(_markdown);
   }

   [Benchmark(Description = "MarkdownSharp")]
   [BenchmarkCategory("Full Spec")]
   public string FullSpecMarkdownSharp()
   {
      return new MarkdownSharp.Markdown().Transform(_markdown);
   }
   
   [Benchmark(Description = "Markdig")]
   [BenchmarkCategory("Small")]
   public string SimpleMarkdig()
   {
      return Markdig.Markdown.ToHtml("Hello, **world**!");
   }

   [Benchmark(Description = "Beskar.Markdown")]
   [BenchmarkCategory("Small")]
   public string SimpleBeMarkdown()
   {
      return BeMarkdown.ToHtml("Hello, **world**!");
   }
   
   [Benchmark(Description = "CommonMark.Net")]
   [BenchmarkCategory("Small")]
   public string SimpleCommonMarkNet()
   {
      return CommonMark.CommonMarkConverter.Convert("Hello, **world**!");
   }

   [Benchmark(Description = "MarkdownSharp")]
   [BenchmarkCategory("Small")]
   public string SimpleMarkdownSharp()
   {
      return new MarkdownSharp.Markdown().Transform("Hello, **world**!");
   }
   
   [Benchmark(Description = "Markdig")]
   [BenchmarkCategory("Bigger")]
   public string BigMarkdig()
   {
      return Markdig.Markdown.ToHtml(_biggerExample);
   }

   [Benchmark(Description = "Beskar.Markdown")]
   [BenchmarkCategory("Bigger")]
   public string BigBeMarkdown()
   {
      return BeMarkdown.ToHtml(_biggerExample);
   }
   
   [Benchmark(Description = "CommonMark.Net")]
   [BenchmarkCategory("Bigger")]
   public string BigCommonMarkNet()
   {
      return CommonMark.CommonMarkConverter.Convert(_biggerExample);
   }

   [Benchmark(Description = "MarkdownSharp")]
   [BenchmarkCategory("Bigger")]
   public string BigMarkdownSharp()
   {
      return new MarkdownSharp.Markdown().Transform(_biggerExample);
   }

   private const string _biggerExample = """
                                         aa
                                         dsads a <https://foo.bar.baz/test?q=hello&id=22&boolean>
                                         dsadsa<http://foo.bar.baz>
                                         ads       <MAILTO:FOO@BAR.BAZ>
                                         
                                         <https://foo.bar.baz/test?q=hello&id=22&boolean>
                                         <MAILTO:FOO@BAR.BAZ>
                                         
                                         Test 4 <b>***hallo***</b> s \*bold\*
                                         
                                         *(*foo*)*
                                         
                                         foo *\**
                                         
                                         __foo__ aa_adsa *aa
                                         
                                         - - - - -
                                         
                                             Code
                                             Block
                                             Spaces
                                             
                                             Hallo
                                         dsad
                                         
                                               Hallo
                                               a
                                               d
                                         
                                         > foo
                                         bar
                                         ===
                                         
                                         Hardbreak spaces test  
                                         Befpre those
                                         
                                         ## Heading Trail ##
                                         ## Heading Trail 2 ###
                                         ## Heading Trail 3 \##
                                         
                                         setex
                                         ===
                                         
                                         setex 2
                                         ---
                                         
                                         ## Heading 2
                                         Test
                                         
                                         - Item 1
                                         - Item 2
                                         - Item 3 **bold** test
                                            * Item 3.1
                                            * Item 3.2
                                         
                                         Hier ist ein Text.
                                         Wie sieht er aus? *bold* aa
                                         
                                         > Hallo
                                         > > Inner Hallo
                                         > > Inner Hallo2
                                         > Hallo2
                                         
                                         -----
                                         
                                         Hier ist ein Text. 2
                                         Wie sieht er aus? 2 `code inline` s
                                         
                                         `css`
                                         
                                         <div>
                                            <span>Test</span>
                                            <!-- Test -->
                                         </div>
                                         
                                         _____
                                         
                                         ```csharp
                                         >
                                         class Program 
                                         {
                                            private string Test = "> **";
                                         }
                                         ```
                                         
                                         Hier title [Link text](https://google.com)
                                         Image: ![Alt text](https://google.com)
                                         
                                         ###### Heading 6
                                         ##### Heading 5
                                         #### Heading 4 *bold extra* a
                                         ### Heading 3
                                         ## Heading 2**dsad**
                                         # Heading 1** * double * **
                                         
                                         Hardbeak \
                                         Test \
                                         
                                         1. First
                                            - aa
                                               - bb
                                               - cc
                                            - aaa
                                         2. Second
                                            * adsa
                                         22. Third
                                         
                                         [link](\(foo\))
                                         [link](foo(and(bar)))
                                         [link](foo(and(bar))
                                         [link](#fragment)
                                         [link]("title")
                                         
                                         ~~strikethrough~~
                                         """;
}
