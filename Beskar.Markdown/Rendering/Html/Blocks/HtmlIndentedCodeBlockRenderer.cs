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
         writer.Write("<pre><code class=\"language-");
         writer.WriteHtmlDecodedAndEncoded(rawText.Slice(current.CodeLangSpanStart, current.CodeLangSpanLength));
         writer.Write("\">");
      }
      else
      {
         writer.Write("<pre><code>");
      }

      var lastValidChildIndex = -1;
      var searchIndex = current.FirstChildIndex;
      
      while (searchIndex != -1)
      {
         var child = nodes[searchIndex];
         if (child.Type == NodeType.IndentedCodeFragment)
         {
            var content = child.TextSpan.Slice(rawText);
            var isBlank = true;
            
            for (var i = 0; i < content.Length; i++)
            {
               var c = content[i];
               if (c != ' ' && c != '\t' && c != '\r' && c != '\n')
               {
                  isBlank = false;
                  break;
               }
            }

            if (!isBlank)
            {
               lastValidChildIndex = searchIndex;
            }
         }
         searchIndex = child.NextSiblingIndex;
      }
      
      var currentChildIndex = current.FirstChildIndex;
      while (currentChildIndex != -1)
      {
         var child = nodes[currentChildIndex];
         if (child.Type is not NodeType.IndentedCodeFragment) continue;

         if (child.TextSpan is { Start: >= 0, Length: > 0 } 
             || child.LeadingVirtualSpaces > 0)
         {
            for (var i = 0; i < child.LeadingVirtualSpaces; i++)
            {
               writer.Write(" ");
            }
            
            writer.WriteHtmlEncoded(child.TextSpan.Slice(rawText), encodeApostrophe: false);
         }
         writer.WriteLine();
         
         if (currentChildIndex == lastValidChildIndex)
         {
            break;
         }
         
         currentChildIndex = child.NextSiblingIndex;
      }

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