using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlCodeBlockRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.CodeBlock;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var span = current.TextSpan;
      var text = span.Slice(rawText);
      
      if (current is { CodeLangSpanStart: > -1, CodeLangSpanLength: > 0 })
      {
         writer.WriteInterpolated($"<pre><code class=\"language-{rawText.Slice(current.CodeLangSpanStart, current.CodeLangSpanLength)}\">");
      }
      else
      {
         writer.Write("<pre><code>");
      }
      
      writer.WriteHtmlEncoded(text, encodeApostrophe: false);

      if (options.AddBlockNewLines)
      {
         writer.WriteLine("</code></pre>");
      }
      else
      {
         writer.Write("</code></pre>");
      }
   }
}