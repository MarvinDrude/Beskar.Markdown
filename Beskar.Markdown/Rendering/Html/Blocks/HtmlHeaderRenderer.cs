using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlHeaderRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Header;

   public void Render(
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.WriteLineInterpolated($"<h{current.HeadingLevel}>{rawText}</h{current.HeadingLevel}>");
   }
}