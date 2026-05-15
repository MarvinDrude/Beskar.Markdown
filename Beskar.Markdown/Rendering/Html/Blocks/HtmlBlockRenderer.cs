using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlBlockRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.HtmlBlock;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var span = current.TextSpan;

      if (options.AddBlockNewLines)
      {
         writer.WriteLine(span.Slice(rawText));
      }
      else
      {
         writer.Write(span.Slice(rawText));
      }
   }
}