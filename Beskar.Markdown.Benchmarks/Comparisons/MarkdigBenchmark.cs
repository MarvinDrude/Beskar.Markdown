using BenchmarkDotNet.Attributes;
using Beskar.Markdown.Benchmarks.Config;

namespace Beskar.Markdown.Benchmarks.Comparisons;

[Config(typeof(WarmupDetailedConfig))]
public class MarkdigBenchmark
{
   private string _markdown = string.Empty;
   
   [GlobalSetup]
   public void Setup()
   {
      _markdown = File.ReadAllText("spec.md");
   }
   
   [Benchmark]
   public string FullSpecMarkdig()
   {
      return Markdig.Markdown.ToHtml(_markdown);
   }

   [Benchmark]
   public string FullSpecBeMarkdown()
   {
      return BeMarkdown.ToHtml(_markdown);
   }
   
   [Benchmark]
   public string SimpleMarkdig()
   {
      return Markdig.Markdown.ToHtml("Hello, **world**!");
   }

   [Benchmark]
   public string SimpleBeMarkdown()
   {
      return BeMarkdown.ToHtml("Hello, **world**!");
   }
   
   [Benchmark]
   public string BigMarkdig()
   {
      return Markdig.Markdown.ToHtml(
         """
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
         """);
   }

   [Benchmark]
   public string BigBeMarkdown()
   {
      return BeMarkdown.ToHtml(
         """
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
         """);
   }
}