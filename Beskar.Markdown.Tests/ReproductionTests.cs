
using Beskar.Markdown.Rendering;

namespace Beskar.Markdown.Tests;

public sealed class ReproductionTests
{
   [Test]
   public Task MultilineParagraphNoTrailingSpace()
   {
      const string markdown = "foo\nbaz";
      const string expectedHtml = "<p>foo\nbaz</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task MultilineHtmlComment()
   {
      const string markdown = "foo <!-- this is a --\ncomment - with hyphens -->";
      const string expectedHtml = "<p>foo <!-- this is a --\ncomment - with hyphens --></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HardLineBreakWithSpaces()
   {
      const string markdown = "foo  \nbaz";
      const string expectedHtml = "<p>foo<br />\nbaz</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HardLineBreakWithTab()
   {
      const string markdown = "foo\t\nbaz";
      const string expectedHtml = "<p>foo<br />\nbaz</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HardLineBreakWithThreeSpaces()
   {
      const string markdown = "foo   \nbaz";
      const string expectedHtml = "<p>foo<br />\nbaz</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }
   
   [Test]
   public Task SpacesBeforeCodeBlock()
   {
      const string markdown = " ```\n aaa\naaa\n```\n";
      const string expectedHtml = "<pre><code>aaa\naaa\n</code></pre>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }
   
   [Test]
   public Task HeaderFrontSpaces()
   {
      const string markdown = "#                  foo                     \n";
      const string expectedHtml = "<h1>foo</h1>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }
   
   [Test]
   public Task EmphasisWithLinkNoEmphasis()
   {
      const string markdown = "*[bar*](/url)\n";
      const string expectedHtml = "<p>*<a href=\"/url\">bar*</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }
   
   [Test]
   public Task EmphasisWithLinkNoEmphasis2()
   {
      const string markdown = "*[bar](/url)*";
      const string expectedHtml = "<p><em><a href=\"/url\">bar</a></em></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task PartialEmphasisMatch()
   {
      const string markdown = "**foo*";
      const string expectedHtml = "<p>*<em>foo</em></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }
   
   [Test]
   public Task MultiLineListItem()
   {
      const string markdown = "  - foo\n\n    bar\n";
      const string expectedHtml = "<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>\n";
   
      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }
   [Test]
   public Task Tabs_5()
   {
      const string markdown = "- foo\n\n\t\tbar\n";
      const string expectedHtml = "<ul>\n<li>\n<p>foo</p>\n<pre><code>  bar\n</code></pre>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Tabs_6()
   {
      const string markdown = ">\t\tfoo\n";
      const string expectedHtml = "<blockquote>\n<pre><code>  foo\n</code></pre>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Tabs_7()
   {
      const string markdown = "-\t\tfoo\n";
      const string expectedHtml = "<ul>\n<li>\n<pre><code>  foo\n</code></pre>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Tabs_9()
   {
      const string markdown = " - foo\n   - bar\n\t - baz\n";
      const string expectedHtml = "<ul>\n<li>foo\n<ul>\n<li>bar\n<ul>\n<li>baz</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Backslashescapes_20()
   {
      const string markdown = "<https://example.com?find=\\*>\n";
      const string expectedHtml = "<p><a href=\"https://example.com?find=%5C*\">https://example.com?find=\\*</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Backslashescapes_21()
   {
      const string markdown = "<a href=\"/bar\\/)\">\n";
      const string expectedHtml = "<a href=\"/bar\\/)\">\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Entityandnumericcharacterreferences_25()
   {
      const string markdown = "&nbsp; &amp; &copy; &AElig; &Dcaron;\n&frac34; &HilbertSpace; &DifferentialD;\n&ClockwiseContourIntegral; &ngE;\n";
      const string expectedHtml = "<p>  &amp; © Æ Ď\n¾ ℋ ⅆ\n∲ ≧̸</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Entityandnumericcharacterreferences_26()
   {
      const string markdown = "&#35; &#1234; &#992; &#0;\n";
      const string expectedHtml = "<p># Ӓ Ϡ �</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Entityandnumericcharacterreferences_28()
   {
      const string markdown = "&nbsp &x; &#; &#x;\n&#87654321;\n&#abcdef0;\n&ThisIsNotDefined; &hi?;\n";
      const string expectedHtml = "<p>&amp;nbsp &amp;x; &amp;#; &amp;#x;\n&amp;#87654321;\n&amp;#abcdef0;\n&amp;ThisIsNotDefined; &amp;hi?;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Entityandnumericcharacterreferences_31()
   {
      const string markdown = "<a href=\"&ouml;&ouml;.html\">\n";
      const string expectedHtml = "<a href=\"&ouml;&ouml;.html\">\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Entityandnumericcharacterreferences_32()
   {
      const string markdown = "[foo](/f&ouml;&ouml; \"f&ouml;&ouml;\")\n";
      const string expectedHtml = "<p><a href=\"/f%C3%B6%C3%B6\" title=\"föö\">foo</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Entityandnumericcharacterreferences_33()
   {
      const string markdown = "[foo]\n\n[foo]: /f&ouml;&ouml; \"f&ouml;&ouml;\"\n";
      const string expectedHtml = "<p><a href=\"/f%C3%B6%C3%B6\" title=\"föö\">foo</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Thematicbreaks_60()
   {
      const string markdown = "* Foo\n* * *\n* Bar\n";
      const string expectedHtml = "<ul>\n<li>Foo</li>\n</ul>\n<hr />\n<ul>\n<li>Bar</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Thematicbreaks_61()
   {
      const string markdown = "- Foo\n- * * *\n";
      const string expectedHtml = "<ul>\n<li>Foo</li>\n<li>\n<hr />\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Setextheadings_91()
   {
      const string markdown = "`Foo\n----\n`\n\n<a title=\"a lot\n---\nof dashes\"/>\n";
      const string expectedHtml = "<h2>`Foo</h2>\n<p>`</p>\n<h2>&lt;a title=&quot;a lot</h2>\n<p>of dashes&quot;/&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Setextheadings_93()
   {
      const string markdown = "> foo\nbar\n===\n";
      const string expectedHtml = "<blockquote>\n<p>foo\nbar\n===</p>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Fencedcodeblocks_128()
   {
      const string markdown = "> ```\n> aaa\n\nbbb\n";
      const string expectedHtml = "<blockquote>\n<pre><code>aaa\n</code></pre>\n</blockquote>\n<p>bbb</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HTMLblocks_148()
   {
      const string markdown = "<table><tr><td>\n<pre>\n**Hello**,\n\n_world_.\n</pre>\n</td></tr></table>\n";
      const string expectedHtml = "<table><tr><td>\n<pre>\n**Hello**,\n<p><em>world</em>.\n</pre></p>\n</td></tr></table>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HTMLblocks_162()
   {
      const string markdown = "<a href=\"foo\">\n*bar*\n</a>\n";
      const string expectedHtml = "<a href=\"foo\">\n*bar*\n</a>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HTMLblocks_171()
   {
      const string markdown = "<textarea>\n\n*foo*\n\n_bar_\n\n</textarea>\n";
      const string expectedHtml = "<textarea>\n\n*foo*\n\n_bar_\n\n</textarea>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HTMLblocks_174()
   {
      const string markdown = "> <div>\n> foo\n\nbar\n";
      const string expectedHtml = "<blockquote>\n<div>\nfoo\n</blockquote>\n<p>bar</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task HTMLblocks_175()
   {
      const string markdown = "- <div>\n- foo\n";
      const string expectedHtml = "<ul>\n<li>\n<div>\n</li>\n<li>foo</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Linkreferencedefinitions_197()
   {
      const string markdown = "[foo]: /url 'title\n\nwith blank line'\n\n[foo]\n";
      const string expectedHtml = "<p>[foo]: /url 'title</p>\n<p>with blank line'</p>\n<p>[foo]</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Linkreferencedefinitions_199()
   {
      const string markdown = "[foo]:\n\n[foo]\n";
      const string expectedHtml = "<p>[foo]:</p>\n<p>[foo]</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Linkreferencedefinitions_201()
   {
      const string markdown = "[foo]: <bar>(baz)\n\n[foo]\n";
      const string expectedHtml = "<p>[foo]: <bar>(baz)</p>\n<p>[foo]</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Linkreferencedefinitions_206()
   {
      const string markdown = "[ΑΓΩ]: /φου\n\n[αγω]\n";
      const string expectedHtml = "<p><a href=\"/%CF%86%CE%BF%CF%85\">αγω</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Linkreferencedefinitions_208()
   {
      const string markdown = "[\nfoo\n]: /url\nbar\n";
      const string expectedHtml = "<p>bar</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Linkreferencedefinitions_209()
   {
      const string markdown = "[foo]: /url \"title\" ok\n";
      const string expectedHtml = "<p>[foo]: /url &quot;title&quot; ok</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Blockquotes_232()
   {
      const string markdown = "> # Foo\n> bar\nbaz\n";
      const string expectedHtml = "<blockquote>\n<h1>Foo</h1>\n<p>bar\nbaz</p>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Blockquotes_233()
   {
      const string markdown = "> bar\nbaz\n> foo\n";
      const string expectedHtml = "<blockquote>\n<p>bar\nbaz\nfoo</p>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Blockquotes_238()
   {
      const string markdown = "> foo\n    - bar\n";
      const string expectedHtml = "<blockquote>\n<p>foo\n- bar</p>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Blockquotes_247()
   {
      const string markdown = "> bar\nbaz\n";
      const string expectedHtml = "<blockquote>\n<p>bar\nbaz</p>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Blockquotes_250()
   {
      const string markdown = "> > > foo\nbar\n";
      const string expectedHtml = "<blockquote>\n<blockquote>\n<blockquote>\n<p>foo\nbar</p>\n</blockquote>\n</blockquote>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Blockquotes_251()
   {
      const string markdown = ">>> foo\n> bar\n>>baz\n";
      const string expectedHtml = "<blockquote>\n<blockquote>\n<blockquote>\n<p>foo\nbar\nbaz</p>\n</blockquote>\n</blockquote>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_255()
   {
      const string markdown = "- one\n\n two\n";
      const string expectedHtml = "<ul>\n<li>one</li>\n</ul>\n<p>two</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_257()
   {
      const string markdown = " -    one\n\n     two\n";
      const string expectedHtml = "<ul>\n<li>one</li>\n</ul>\n<pre><code> two\n</code></pre>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_263()
   {
      const string markdown = "1.  foo\n\n    ```\n    bar\n    ```\n\n    baz\n\n    > bam\n";
      const string expectedHtml = "<ol>\n<li>\n<p>foo</p>\n<pre><code>bar\n</code></pre>\n<p>baz</p>\n<blockquote>\n<p>bam</p>\n</blockquote>\n</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_266()
   {
      const string markdown = "1234567890. not ok\n";
      const string expectedHtml = "<p>1234567890. not ok</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_267()
   {
      const string markdown = "0. ok\n";
      const string expectedHtml = "<ol start=\"0\">\n<li>ok</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_273()
   {
      const string markdown = "1.     indented code\n\n   paragraph\n\n       more code\n";
      const string expectedHtml = "<ol>\n<li>\n<pre><code>indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code\n</code></pre>\n</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_274()
   {
      const string markdown = "1.      indented code\n\n   paragraph\n\n       more code\n";
      const string expectedHtml = "<ol>\n<li>\n<pre><code> indented code\n</code></pre>\n<p>paragraph</p>\n<pre><code>more code\n</code></pre>\n</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_276()
   {
      const string markdown = "-    foo\n\n  bar\n";
      const string expectedHtml = "<ul>\n<li>foo</li>\n</ul>\n<p>bar</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_278()
   {
      const string markdown = "-\n  foo\n-\n  ```\n  bar\n  ```\n-\n      baz\n";
      const string expectedHtml = "<ul>\n<li>foo</li>\n<li>\n<pre><code>bar\n</code></pre>\n</li>\n<li>\n<pre><code>baz\n</code></pre>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_279()
   {
      const string markdown = "-   \n  foo\n";
      const string expectedHtml = "<ul>\n<li>foo</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_280()
   {
      const string markdown = "-\n\n  foo\n";
      const string expectedHtml = "<ul>\n<li></li>\n</ul>\n<p>foo</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_291()
   {
      const string markdown = "  1.  A paragraph\n    with two lines.\n";
      const string expectedHtml = "<ol>\n<li>A paragraph\nwith two lines.</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_292()
   {
      const string markdown = "> 1. > Blockquote\ncontinued here.\n";
      const string expectedHtml = "<blockquote>\n<ol>\n<li>\n<blockquote>\n<p>Blockquote\ncontinued here.</p>\n</blockquote>\n</li>\n</ol>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_293()
   {
      const string markdown = "> 1. > Blockquote\n> continued here.\n";
      const string expectedHtml = "<blockquote>\n<ol>\n<li>\n<blockquote>\n<p>Blockquote\ncontinued here.</p>\n</blockquote>\n</li>\n</ol>\n</blockquote>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_294()
   {
      const string markdown = "- foo\n  - bar\n    - baz\n      - boo\n";
      const string expectedHtml = "<ul>\n<li>foo\n<ul>\n<li>bar\n<ul>\n<li>baz\n<ul>\n<li>boo</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_296()
   {
      const string markdown = "10) foo\n    - bar\n";
      const string expectedHtml = "<ol start=\"10\">\n<li>foo\n<ul>\n<li>bar</li>\n</ul>\n</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_297()
   {
      const string markdown = "10) foo\n   - bar\n";
      const string expectedHtml = "<ol start=\"10\">\n<li>foo</li>\n</ol>\n<ul>\n<li>bar</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_298()
   {
      const string markdown = "- - foo\n";
      const string expectedHtml = "<ul>\n<li>\n<ul>\n<li>foo</li>\n</ul>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_299()
   {
      const string markdown = "1. - 2. foo\n";
      const string expectedHtml = "<ol>\n<li>\n<ul>\n<li>\n<ol start=\"2\">\n<li>foo</li>\n</ol>\n</li>\n</ul>\n</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Listitems_300()
   {
      const string markdown = "- # Foo\n- Bar\n  ---\n  baz\n";
      const string expectedHtml = "<ul>\n<li>\n<h1>Foo</h1>\n</li>\n<li>\n<h2>Bar</h2>\nbaz</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_306()
   {
      const string markdown = "- foo\n\n- bar\n\n\n- baz\n";
      const string expectedHtml = "<ul>\n<li>\n<p>foo</p>\n</li>\n<li>\n<p>bar</p>\n</li>\n<li>\n<p>baz</p>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_307()
   {
      const string markdown = "- foo\n  - bar\n    - baz\n\n\n      bim\n";
      const string expectedHtml = "<ul>\n<li>foo\n<ul>\n<li>bar\n<ul>\n<li>\n<p>baz</p>\n<p>bim</p>\n</li>\n</ul>\n</li>\n</ul>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_309()
   {
      const string markdown = "-   foo\n\n    notcode\n\n-   foo\n\n<!-- -->\n\n    code\n";
      const string expectedHtml = "<ul>\n<li>\n<p>foo</p>\n<p>notcode</p>\n</li>\n<li>\n<p>foo</p>\n</li>\n</ul>\n<!-- -->\n<pre><code>code\n</code></pre>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_311()
   {
      const string markdown = "1. a\n\n  2. b\n\n   3. c\n";
      const string expectedHtml = "<ol>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_312()
   {
      const string markdown = "- a\n - b\n  - c\n   - d\n    - e\n";
      const string expectedHtml = "<ul>\n<li>a</li>\n<li>b</li>\n<li>c</li>\n<li>d\n- e</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_313()
   {
      const string markdown = "1. a\n\n  2. b\n\n    3. c\n";
      const string expectedHtml = "<ol>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n</ol>\n<pre><code>3. c\n</code></pre>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_314()
   {
      const string markdown = "- a\n- b\n\n- c\n";
      const string expectedHtml = "<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>c</p>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_315()
   {
      const string markdown = "* a\n*\n\n* c\n";
      const string expectedHtml = "<ul>\n<li>\n<p>a</p>\n</li>\n<li></li>\n<li>\n<p>c</p>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_316()
   {
      const string markdown = "- a\n- b\n\n  c\n- d\n";
      const string expectedHtml = "<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n<p>c</p>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_317()
   {
      const string markdown = "- a\n- b\n\n  [ref]: /url\n- d\n";
      const string expectedHtml = "<ul>\n<li>\n<p>a</p>\n</li>\n<li>\n<p>b</p>\n</li>\n<li>\n<p>d</p>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_318()
   {
      const string markdown = "- a\n- ```\n  b\n\n\n  ```\n- c\n";
      const string expectedHtml = "<ul>\n<li>a</li>\n<li>\n<pre><code>b\n\n\n</code></pre>\n</li>\n<li>c</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_319()
   {
      const string markdown = "- a\n  - b\n\n    c\n- d\n";
      const string expectedHtml = "<ul>\n<li>a\n<ul>\n<li>\n<p>b</p>\n<p>c</p>\n</li>\n</ul>\n</li>\n<li>d</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_320()
   {
      const string markdown = "* a\n  > b\n  >\n* c\n";
      const string expectedHtml = "<ul>\n<li>a\n<blockquote>\n<p>b</p>\n</blockquote>\n</li>\n<li>c</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_321()
   {
      const string markdown = "- a\n  > b\n  ```\n  c\n  ```\n- d\n";
      const string expectedHtml = "<ul>\n<li>a\n<blockquote>\n<p>b</p>\n</blockquote>\n<pre><code>c\n</code></pre>\n</li>\n<li>d</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_323()
   {
      const string markdown = "- a\n  - b\n";
      const string expectedHtml = "<ul>\n<li>a\n<ul>\n<li>b</li>\n</ul>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_324()
   {
      const string markdown = "1. ```\n   foo\n   ```\n\n   bar\n";
      const string expectedHtml = "<ol>\n<li>\n<pre><code>foo\n</code></pre>\n<p>bar</p>\n</li>\n</ol>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Lists_326()
   {
      const string markdown = "- a\n  - b\n  - c\n\n- d\n  - e\n  - f\n";
      const string expectedHtml = "<ul>\n<li>\n<p>a</p>\n<ul>\n<li>b</li>\n<li>c</li>\n</ul>\n</li>\n<li>\n<p>d</p>\n<ul>\n<li>e</li>\n<li>f</li>\n</ul>\n</li>\n</ul>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Codespans_342()
   {
      const string markdown = "[not a `link](/foo`)\n";
      const string expectedHtml = "<p>[not a <code>link](/foo</code>)</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Codespans_346()
   {
      const string markdown = "<https://foo.bar.`baz>`\n";
      const string expectedHtml = "<p><a href=\"https://foo.bar.%60baz\">https://foo.bar.`baz</a>`</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Emphasis_Nested_Simple()
   {
      const string markdown = "*_foo_*";
      const string expectedHtml = "<p><em><em>foo</em></em></p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task Emphasisandstrongemphasis_353()
   {
      const string markdown = "* a *\n";
      const string expectedHtml = "<p>* a *</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Emphasisandstrongemphasis_461()
   {
      const string markdown = "*_foo_*\n";
      const string expectedHtml = "<p><em><em>foo</em></em></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Emphasisandstrongemphasis_463()
   {
      const string markdown = "_*foo*_\n";
      const string expectedHtml = "<p><em><em>foo</em></em></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_491()
   {
      const string markdown = "[link](<foo\nbar>)\n";
      const string expectedHtml = "<p>[link](<foo\nbar>)</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_492()
   {
      const string markdown = "[a](<b)c>)\n";
      const string expectedHtml = "<p><a href=\"b)c\">a</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_493()
   {
      const string markdown = "[link](<foo\\>)\n";
      const string expectedHtml = "<p>[link](&lt;foo&gt;)</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_494()
   {
      const string markdown = "[a](<b)c\n[a](<b)c>\n[a](<b>c)\n";
      const string expectedHtml = "<p>[a](&lt;b)c\n[a](&lt;b)c&gt;\n[a](<b>c)</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_503()
   {
      const string markdown = "[link](foo%20b&auml;)\n";
      const string expectedHtml = "<p><a href=\"foo%20b%C3%A4\">link</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_506()
   {
      const string markdown = "[link](/url \"title \\\"&quot;\")\n";
      const string expectedHtml = "<p><a href=\"/url\" title=\"title &quot;&quot;\">link</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_507()
   {
      const string markdown = "[link](/url \"title\")\n";
      const string expectedHtml = "<p><a href=\"/url%C2%A0%22title%22\">link</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_510()
   {
      const string markdown = "[link](   /uri\n  \"title\"  )\n";
      const string expectedHtml = "<p><a href=\"/uri\" title=\"title\">link</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_518()
   {
      const string markdown = "[foo [bar](/uri)](/uri)\n";
      const string expectedHtml = "<p>[foo <a href=\"/uri\">bar</a>](/uri)</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_519()
   {
      const string markdown = "[foo *[bar [baz](/uri)](/uri)*](/uri)\n";
      const string expectedHtml = "<p>[foo <em>[bar <a href=\"/uri\">baz</a>](/uri)</em>](/uri)</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_520()
   {
      const string markdown = "![[[foo](uri1)](uri2)](uri3)\n";
      const string expectedHtml = "<p><img src=\"uri3\" alt=\"[foo](uri2)\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_524()
   {
      const string markdown = "[foo <bar attr=\"](baz)\">\n";
      const string expectedHtml = "<p>[foo <bar attr=\"](baz)\"></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_525()
   {
      const string markdown = "[foo`](/uri)`\n";
      const string expectedHtml = "<p>[foo<code>](/uri)</code></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_526()
   {
      const string markdown = "[foo<https://example.com/?search=](uri)>\n";
      const string expectedHtml = "<p>[foo<a href=\"https://example.com/?search=%5D(uri)\">https://example.com/?search=](uri)</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_532()
   {
      const string markdown = "[foo [bar](/uri)][ref]\n\n[ref]: /uri\n";
      const string expectedHtml = "<p>[foo <a href=\"/uri\">bar</a>]<a href=\"/uri\">ref</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_533()
   {
      const string markdown = "[foo *bar [baz][ref]*][ref]\n\n[ref]: /uri\n";
      const string expectedHtml = "<p>[foo <em>bar <a href=\"/uri\">baz</a></em>]<a href=\"/uri\">ref</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_536()
   {
      const string markdown = "[foo <bar attr=\"][ref]\">\n\n[ref]: /uri\n";
      const string expectedHtml = "<p>[foo <bar attr=\"][ref]\"></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_537()
   {
      const string markdown = "[foo`][ref]`\n\n[ref]: /uri\n";
      const string expectedHtml = "<p>[foo<code>][ref]</code></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_538()
   {
      const string markdown = "[foo<https://example.com/?search=][ref]>\n\n[ref]: /uri\n";
      const string expectedHtml = "<p>[foo<a href=\"https://example.com/?search=%5D%5Bref%5D\">https://example.com/?search=][ref]</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_541()
   {
      const string markdown = "[Foo\n  bar]: /url\n\n[Baz][Foo bar]\n";
      const string expectedHtml = "<p><a href=\"/url\">Baz</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Links_568()
   {
      const string markdown = "[foo](not a link)\n\n[foo]: /url1\n";
      const string expectedHtml = "<p><a href=\"/url1\">foo</a>(not a link)</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Images_573()
   {
      const string markdown = "![foo *bar*]\n\n[foo *bar*]: train.jpg \"train & tracks\"\n";
      const string expectedHtml = "<p><img src=\"train.jpg\" alt=\"foo bar\" title=\"train &amp; tracks\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Images_574()
   {
      const string markdown = "![foo ![bar](/url)](/url2)\n";
      const string expectedHtml = "<p><img src=\"/url2\" alt=\"foo bar\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Images_575()
   {
      const string markdown = "![foo [bar](/url)](/url2)\n";
      const string expectedHtml = "<p><img src=\"/url2\" alt=\"foo bar\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Images_576()
   {
      const string markdown = "![foo *bar*][]\n\n[foo *bar*]: train.jpg \"train & tracks\"\n";
      const string expectedHtml = "<p><img src=\"train.jpg\" alt=\"foo bar\" title=\"train &amp; tracks\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Images_577()
   {
      const string markdown = "![foo *bar*][foobar]\n\n[FOOBAR]: train.jpg \"train & tracks\"\n";
      const string expectedHtml = "<p><img src=\"train.jpg\" alt=\"foo bar\" title=\"train &amp; tracks\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Images_585()
   {
      const string markdown = "![*foo* bar][]\n\n[*foo* bar]: /url \"title\"\n";
      const string expectedHtml = "<p><img src=\"/url\" alt=\"foo bar\" title=\"title\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Images_589()
   {
      const string markdown = "![*foo* bar]\n\n[*foo* bar]: /url \"title\"\n";
      const string expectedHtml = "<p><img src=\"/url\" alt=\"foo bar\" title=\"title\" /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Autolinks_597()
   {
      const string markdown = "<MAILTO:FOO@BAR.BAZ>\n";
      const string expectedHtml = "<p><a href=\"MAILTO:FOO@BAR.BAZ\">MAILTO:FOO@BAR.BAZ</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Autolinks_602()
   {
      const string markdown = "<https://foo.bar/baz bim>\n";
      const string expectedHtml = "<p>&lt;https://foo.bar/baz bim&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Autolinks_603()
   {
      const string markdown = "<https://example.com/\\[\\>\n";
      const string expectedHtml = "<p><a href=\"https://example.com/%5C%5B%5C\">https://example.com/\\[\\</a></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Autolinks_606()
   {
      const string markdown = "<foo\\+@bar.example.com>\n";
      const string expectedHtml = "<p>&lt;foo+@bar.example.com&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Autolinks_609()
   {
      const string markdown = "<m:abc>\n";
      const string expectedHtml = "<p>&lt;m:abc&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task Autolinks_610()
   {
      const string markdown = "<foo.bar.baz>\n";
      const string expectedHtml = "<p>&lt;foo.bar.baz&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task RawHTML_616()
   {
      const string markdown = "<a foo=\"bar\" bam = 'baz <em>\"</em>'\n_boolean zoop:33=zoop:33 />\n";
      const string expectedHtml = "<p><a foo=\"bar\" bam = 'baz <em>\"</em>'\n_boolean zoop:33=zoop:33 /></p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task RawHTML_619()
   {
      const string markdown = "<a h*#ref=\"hi\">\n";
      const string expectedHtml = "<p>&lt;a h*#ref=&quot;hi&quot;&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task RawHTML_620()
   {
      const string markdown = "<a href=\"hi'> <a href=hi'>\n";
      const string expectedHtml = "<p>&lt;a href=&quot;hi'&gt; &lt;a href=hi'&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task RawHTML_621()
   {
      const string markdown = "< a><\nfoo><bar/ >\n<foo bar=baz\nbim!bop />\n";
      const string expectedHtml = "<p>&lt; a&gt;&lt;\nfoo&gt;&lt;bar/ &gt;\n&lt;foo bar=baz\nbim!bop /&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task RawHTML_622()
   {
      const string markdown = "<a href='bar'title=title>\n";
      const string expectedHtml = "<p>&lt;a href='bar'title=title&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task RawHTML_624()
   {
      const string markdown = "</a href=\"foo\">\n";
      const string expectedHtml = "<p>&lt;/a href=&quot;foo&quot;&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

   [Test]
   public Task RawHTML_632()
   {
      const string markdown = "<a href=\"\\\"\">\n";
      const string expectedHtml = "<p>&lt;a href=&quot;&quot;&quot;&gt;</p>\n";

      var renderOptions = RenderOptions.HtmlDefault;
      return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   }

}
