using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

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
      var language = current is { CodeLangSpanStart: > -1, CodeLangSpanLength: > 0 }
         ? rawText.Slice(current.CodeLangSpanStart, current.CodeLangSpanLength)
         : [];

      if (options.CodeBlockRenderer != null)
      {
         using var buffer = new BufferWriter<char>(current.TextSpan.Length);
         var hookLastValidChildIndex = -1;
         var hookSearchIndex = current.FirstChildIndex;

         while (hookSearchIndex != -1)
         {
            var child = nodes[hookSearchIndex];
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
                  hookLastValidChildIndex = hookSearchIndex;
               }
            }

            hookSearchIndex = child.NextSiblingIndex;
         }

         var hookCurrentChildIndex = current.FirstChildIndex;
         while (hookCurrentChildIndex != -1)
         {
            var child = nodes[hookCurrentChildIndex];
            if (child.Type is not NodeType.IndentedCodeFragment) continue;

            if (child.TextSpan is { Start: >= 0, Length: > 0 }
                || child.LeadingVirtualSpaces > 0)
            {
               for (var i = 0; i < child.LeadingVirtualSpaces; i++)
               {
                  buffer.Write([' ']);
               }

               buffer.Write(child.TextSpan.Slice(rawText));
            }

            buffer.Write(['\n']);

            if (hookCurrentChildIndex == hookLastValidChildIndex)
            {
               break;
            }

            hookCurrentChildIndex = child.NextSiblingIndex;
         }

         if (options.CodeBlockRenderer.TryRender(
                context, ref writer, buffer.WrittenSpan, language))
         {
            return;
         }
      }

      if (language.Length > 0)
      {
         writer.Write("<pre><code class=\"language-");
         writer.WriteHtmlDecodedAndEncoded(language);
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