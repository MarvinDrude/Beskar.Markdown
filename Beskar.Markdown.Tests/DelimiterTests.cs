using Beskar.Markdown.Rendering;

namespace Beskar.Markdown.Tests;

public sealed class DelimiterTests
{
    [Test]
    public Task EmphasisRuleOfThree_NoMatch()
    {
        // a*b** -> a*b** (Sum 3, neither mult of 3)
        const string markdown = "a*b**";
        const string expectedHtml = "<p>a*b**</p>\n";

        var renderOptions = RenderOptions.HtmlDefault;
        return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
    }

    [Test]
    public Task EmphasisRuleOfThree_Match()
    {
        // a*b*** -> <em>a</em>b** (Sum 4, OK)
        const string markdown = "a*b***";
        const string expectedHtml = "<p>a<em>b</em>**</p>\n";

        var renderOptions = RenderOptions.HtmlDefault;
        return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
    }

    [Test]
    public Task ComplexEmphasisNesting()
    {
        // ***foo** bar* -> <em><strong>foo</strong> bar</em>
        const string markdown = "***foo** bar*";
        const string expectedHtml = "<p><em><strong>foo</strong> bar</em></p>\n";

        var renderOptions = RenderOptions.HtmlDefault;
        return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
    }
}
