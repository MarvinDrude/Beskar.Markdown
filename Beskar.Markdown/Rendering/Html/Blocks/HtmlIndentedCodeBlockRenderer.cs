using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlIndentedCodeBlockRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.IndentedCodeBlock;

   public void Render(
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.Write("<pre><code>");
      
      var currentChildIndex = current.FirstChildIndex;

      while (currentChildIndex != -1)
      {
         var child = nodes[currentChildIndex];
         if (child.Type is not NodeType.IndentedCodeFragment) continue;
         
         writer.WriteLine(child.TextSpan.Slice(rawText));
         currentChildIndex = child.NextSiblingIndex;
      }
      
      writer.Write("</code></pre>");
   }
}