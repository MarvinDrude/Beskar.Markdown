using Beskar.Markdown.Parsing.Utils;
using Beskar.Markdown.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Extensions;

public static class TextWriterIndentSlimExtensions
{
   extension(ref TextWriterIndentSlim writer)
   {
      public void WriteHtmlDecodedAndEncoded(scoped ReadOnlySpan<char> text, bool encodeApostrophe = true)
      {
         if (text.IsEmpty) return;

         var lastIndex = 0;
         Span<char> decoded = stackalloc char[1];
         
         for (var i = 0; i < text.Length; i++)
         {
            var c = text[i];
            
            if (c == '\\' && i + 1 < text.Length)
            {
               var next = text[i + 1];
               if (LinkUtils.IsAsciiPunctuation(next))
               {
                  if (i > lastIndex) writer.Write(text[lastIndex..i]);

                  writer.WriteCharHtmlEncoded(next);
                  
                  i++;
                  lastIndex = i + 1;
                  continue;
               }
            }
            else if (c == '&')
            {
               var decodedChar = SpanUtils.TryParseEntity(text[i..], out var consumed);
               if (consumed > 0)
               {
                  if (i > lastIndex) 
                     writer.Write(text[lastIndex..i]);

                  writer.WriteCharHtmlEncoded(decodedChar);
                  
                  i += consumed - 1;
                  lastIndex = i + 1;
                  continue;
               }
            }

            // Normal encoding logic for non-entity characters
            ReadOnlySpan<char> entity = GetEntity(c);

            if (!entity.IsEmpty)
            {
               if (i > lastIndex) writer.Write(text[lastIndex..i]);
               writer.Write(entity);
               
               lastIndex = i + 1;
            }
         }

         if (lastIndex < text.Length) 
            writer.Write(text[lastIndex..]);
         return;

         ReadOnlySpan<char> GetEntity(char c) => c switch
         {
            '<' => "&lt;",
            '>' => "&gt;",
            '&' => "&amp;",
            '"' => "&quot;",
            '\'' when encodeApostrophe => "&#39;",
            _ => []
         };
      }
      
      private void WriteCharHtmlEncoded(char c)
      {
         Span<char> buffer = stackalloc char[1];
         
         switch (c)
         {
            case '<': writer.Write("&lt;"); break;
            case '>': writer.Write("&gt;"); break;
            case '"': writer.Write("&quot;"); break;
            case '&': writer.Write("&amp;"); break;
            default:
               buffer[0] = c;
               writer.Write(buffer);
               break;
         }
      }
   }
}