using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

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
      var language = current is { CodeLangSpanStart: > -1, CodeLangSpanLength: > 0 }
         ? rawText.Slice(current.CodeLangSpanStart, current.CodeLangSpanLength)
         : [];

      if (options.CodeBlockRenderer is not null)
      {
         if (current.CodeBlockIndent <= 0)
         {
            if (options.CodeBlockRenderer.TryRender(context, ref writer, text, language))
            {
               return;
            }
         }
         else
         {
            using var buffer = new BufferWriter<char>(text.Length);
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
                  if (c == ' ') spacesRemoved++;
                  else if (c == '\t') spacesRemoved += 4;
                  else break;
               }

               if (spacesRemoved > indentToRemove)
               {
                  for (var i = 0; i < spacesRemoved - indentToRemove; i++) buffer.Write([' ']);
               }

               if (charIndex < lineSpan.Length)
               {
                  buffer.Write(lineSpan[charIndex..]);
               }
               buffer.Write(['\n']);
            }

            if (options.CodeBlockRenderer.TryRender(context, ref writer, buffer.WrittenSpan, language))
            {
               return;
            }
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
