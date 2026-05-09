using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlCodeBlockRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.CodeBlock;

   public void Render(
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var span = current.TextSpan;
      
      writer.Write("<pre><code>");
      writer.WriteHtmlEncoded(span.Slice(rawText));
      writer.Write("</code></pre>");
   }
}