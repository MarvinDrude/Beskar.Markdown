using Beskar.Markdown.Builders;
using Beskar.Markdown.Rendering;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Addons.Interfaces;
using Beskar.Markdown.Addons;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;
using Beskar.Markdown.Parsing;

namespace Beskar.Markdown.Tests.Common;

public sealed class MarkdownOptionBuilderTests
{
    [Test]
    public async Task Build_WithDefaultOptions_ReturnsExpectedDefaults()
    {
        var options = MarkdownOptionBuilder.Create().Build();

        await Assert.That(options.ParserOptions.MaxBlockDepth).IsEqualTo(16);
        await Assert.That(options.RenderOptions.PerserveSoftBreaks).IsTrue();
        await Assert.That(options.RenderOptions.SanitizerFunc).IsNull();
    }

    [Test]
    public async Task WithMaxBlockDepth_SetsMaxBlockDepth()
    {
        var options = MarkdownOptionBuilder.Create()
            .WithMaxBlockDepth(10)
            .Build();

        await Assert.That(options.ParserOptions.MaxBlockDepth).IsEqualTo(10);
    }

    [Test]
    public async Task WithSanitizer_SetsSanitizerFunc()
    {
        Func<ReadOnlySpan<char>, string> sanitizer = (span) => span.ToString();
        var options = MarkdownOptionBuilder.Create()
            .WithSanitizer(sanitizer)
            .Build();

        await Assert.That(options.RenderOptions.SanitizerFunc).IsEqualTo(sanitizer);
    }

    [Test]
    public async Task WithPerserveSoftBreaks_SetsPerserveSoftBreaks()
    {
        var options = MarkdownOptionBuilder.Create()
            .WithPerserveSoftBreaks(false)
            .Build();

        await Assert.That(options.RenderOptions.PerserveSoftBreaks).IsFalse();
    }

    [Test]
    public async Task WithExtension_AddsExtensionComponents()
    {
        var extension = new MockExtension();
        var options = MarkdownOptionBuilder.Create()
            .WithExtension(extension)
            .Build();

        var renderers = options.RenderOptions.NodeRenderers.ToArray();
        await Assert.That(renderers).Contains(extension.Renderers[0]);
        
        var inlineParsers = options.ParserOptions.InlineParsers.ToArray();
        await Assert.That(inlineParsers).Contains(extension.Parsers[0]);
    }

    [Test]
    public async Task WithExtensions_AddsMultipleExtensions()
    {
        var ext1 = new MockExtension();
        var ext2 = new MockBlockExtension();
        var options = MarkdownOptionBuilder.Create()
            .WithExtensions(ext1, ext2)
            .Build();

        var renderers = options.RenderOptions.NodeRenderers.ToArray();
        await Assert.That(renderers).Contains(ext1.Renderers[0]);
        await Assert.That(renderers).Contains(ext2.Renderers[0]);

        var inlineParsers = options.ParserOptions.InlineParsers.ToArray();
        await Assert.That(inlineParsers).Contains(ext1.Parsers[0]);

        var blockParsers = options.ParserOptions.BlockParsers.ToArray();
        await Assert.That(blockParsers).Contains(ext2.Parsers[0]);
    }

    private class MockExtension : BaseInlineExtension
    {
        public MockExtension()
        {
            Parsers = [new MockInlineParser()];
            Renderers = [new MockRenderer()];
        }
    }

    private class MockBlockExtension : BaseBlockExtension
    {
        public MockBlockExtension()
        {
            Parsers = [new MockBlockParser()];
            Renderers = [new MockBlockRenderer()];
        }
    }

    private class MockInlineParser : IInlineParser
    {
        public int Priority => 0;
        public int SupportedTypeValue => 1000;
        public char TriggerChar => 'm';
        public char TriggerAltChar => 'm';
        public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser parser, ParserOptions options) => false;
    }

    private class MockBlockParser : IBlockParser
    {
        public int Priority => 0;
        public int SupportedTypeValue => 1001;
        public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer) => -1;
        public bool CanContinue(ref MarkdownNode node, ref LineState state, ref BufferWriter<MarkdownNode> writer) => false;
    }

    private class MockRenderer : INodeRenderer
    {
        public int TargetTypeValue => 1000;
        public void Render(ReadOnlySpan<char> rawText, ref TextWriterIndentSlim writer, in MarkdownNode current, ReadOnlySpan<MarkdownNode> nodes, RenderOptions options) { }
    }

    private class MockBlockRenderer : INodeRenderer
    {
        public int TargetTypeValue => 1001;
        public void Render(ReadOnlySpan<char> rawText, ref TextWriterIndentSlim writer, in MarkdownNode current, ReadOnlySpan<MarkdownNode> nodes, RenderOptions options) { }
    }
}
