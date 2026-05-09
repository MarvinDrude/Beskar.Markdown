using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlInlineCodeRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.InlineCode;

   public void Render(
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.Write("<code>");
      writer.WriteHtmlEncoded(current.TextSpan.Slice(rawText));
      writer.Write("</code>");
   }
}