using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Blocks;

public sealed class PlainParagraphRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Paragraph;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      current.RenderChildren(context, rawText, nodes, ref writer, options);

      if (current.IsInsideListItem != 1 || current.ParagraphIsWrapped == 1)
      {
         if (options.AddBlockNewLines)
         {
            writer.WriteLine();
         }
      }
      else if (options.AddBlockNewLines && current.NextSiblingIndex != -1)
      {
         writer.WriteLine();
      }
   }
}
