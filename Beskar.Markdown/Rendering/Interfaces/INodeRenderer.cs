using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Interfaces;

public interface INodeRenderer
{
   public int TargetTypeValue { get; }

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText,
      ref TextWriterIndentSlim writer,
      in MarkdownNode current,
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options);
}