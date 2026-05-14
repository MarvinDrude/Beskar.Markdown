using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlIndentedCodeBlockRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.IndentedCodeBlock;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      if (current is { CodeLangSpanStart: > -1, CodeLangSpanLength: > 0 })
      {
         writer.WriteInterpolated($"<pre><code class=\"language-{rawText.Slice(current.CodeLangSpanStart, current.CodeLangSpanLength)}\">");
      }
      else
      {
         writer.Write("<pre><code>");
      }
      
      var currentChildIndex = current.FirstChildIndex;

      while (currentChildIndex != -1)
      {
         var child = nodes[currentChildIndex];
         if (child.Type is not NodeType.IndentedCodeFragment) continue;

         if (child.TextSpan is { Start: >= 0, Length: > 0 })
         {
            writer.WriteHtmlEncoded(child.TextSpan.Slice(rawText));
         }
         writer.WriteLine();
         
         currentChildIndex = child.NextSiblingIndex;
      }
      
      writer.Write("</code></pre>");
   }
}