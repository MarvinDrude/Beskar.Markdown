using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Blocks;

public sealed class PlainThematicBreakRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.ThematicBreak;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      if (options.AddBlockNewLines)
      {
         writer.WriteLine("---");
      }
      else
      {
         writer.Write("---");
      }
   }
}
