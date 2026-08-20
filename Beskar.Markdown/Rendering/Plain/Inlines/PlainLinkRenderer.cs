using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Inlines;

public sealed class PlainLinkRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Link;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var url = rawText.Slice(current.LinkUrlStart, current.LinkUrlLength);

      if (current.FirstChildIndex == -1)
      {
         writer.Write(url);
         return;
      }

      var beforeCount = writer.WrittenSpan.Length;
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      var linkText = writer.WrittenSpan[beforeCount..];

      if (!url.IsEmpty && !linkText.SequenceEqual(url))
      {
         writer.Write(" (");
         writer.Write(url);
         writer.Write(")");
      }
   }
}
