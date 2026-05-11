using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlLinkReferenceDefinitionRenderer : INodeRenderer
{
    public int TargetTypeValue => (int)NodeType.LinkReferenceDefinition;

    public void Render<TData>(
        MarkdownContext<TData> context,
        ReadOnlySpan<char> rawText, 
        ref TextWriterIndentSlim writer, 
        in MarkdownNode current,
        ReadOnlySpan<MarkdownNode> nodes,
        RenderOptions options)
    {
        // Link reference definitions are not rendered
    }
}
