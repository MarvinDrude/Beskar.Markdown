using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Interfaces;

/// <summary>
/// Defines an optional renderer for code blocks and indented code blocks.
/// </summary>
public interface ICodeBlockRenderer
{
   /// <summary>
   /// Attempts to render the code block.
   /// </summary>
   /// <typeparam name="TData">The type of the context data.</typeparam>
   /// <param name="context">The markdown context.</param>
   /// <param name="writer">The text writer.</param>
   /// <param name="code">The code content.</param>
   /// <param name="language">The language of the code block, if any.</param>
   /// <returns>True if the code block was rendered; otherwise, false to use the default rendering.</returns>
   public bool TryRender<TData>(
      MarkdownContext<TData> context,
      ref TextWriterIndentSlim writer,
      ReadOnlySpan<char> code,
      ReadOnlySpan<char> language);
}
