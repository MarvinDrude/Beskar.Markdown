using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlHeaderRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Header;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.WriteInterpolated($"<h{current.HeadingLevel}>");
      current.RenderChildren(context, rawText, nodes, ref writer, options);

      if (options.AddBlockNewLines)
      {
         writer.WriteLineInterpolated($"</h{current.HeadingLevel}>");
      }
      else
      {
         writer.WriteInterpolated($"</h{current.HeadingLevel}>");
      }
   }
}