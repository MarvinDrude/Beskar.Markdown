using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
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
         var slice = rawText.Slice(current.CodeLangSpanStart, current.CodeLangSpanLength);
         writer.Write($"<pre><code class=\"language-");
         writer.WriteHtmlDecodedAndEncoded(slice);
         writer.Write("\">");
      }
      else
      {
         writer.Write("<pre><code>");
      }

      if (current.CodeBlockIndent <= 0)
      {
         writer.WriteHtmlEncoded(text, encodeApostrophe: false);
         if (!text.IsEmpty
             && span.Start + span.Length < rawText.Length
             && rawText[span.Start + span.Length] is '\r' or '\n'
             && text[^1] is not ('\r' or '\n'))
         {
            writer.WriteLine();
         }
      }
      else
      {
         // move line by line to cut off the initial indent
         var lineIterator = new LineIterator(text);
         while (lineIterator.TryMoveNext(context, out var line))
         {
            var lineSpan = line.RawLine;
            
            var spacesRemoved = 0;
            var charIndex = 0;
            var indentToRemove = current.CodeBlockIndent;

            for (; charIndex < lineSpan.Length && spacesRemoved < indentToRemove; charIndex++)
            {
               var c = lineSpan[charIndex];
               if (c == ' ')
               {
                  spacesRemoved++;
               }
               else if (c == '\t')
               {
                  spacesRemoved += 4;
               }
               else
               {
                  break;
               }
            }

            if (spacesRemoved > indentToRemove)
            {
               var remainingSpaces = spacesRemoved - indentToRemove;
               for (var i = 0; i < remainingSpaces; i++)
               {
                  writer.Write(" ");
               }
            }

            if (charIndex < lineSpan.Length)
            {
               writer.WriteHtmlEncoded(lineSpan[charIndex..], encodeApostrophe: false);
               writer.WriteLine();
            }
         }
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
