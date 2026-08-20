using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Inlines;

public sealed class PlainStrikeThroughRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.StrikeThrough;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      current.RenderChildren(context, rawText, nodes, ref writer, options);
   }
}
