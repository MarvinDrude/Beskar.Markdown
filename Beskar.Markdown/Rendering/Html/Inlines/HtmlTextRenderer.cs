using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlTextRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Text;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.WriteHtmlDecodedAndEncoded(current.TextSpan.Slice(rawText), encodeApostrophe: false);
   }
}