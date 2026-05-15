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
            if (c == '&')
            {
               var decodedChar = SpanUtils.TryParseEntity(text[i..], out var consumed);
               if (consumed > 0)
               {
                  if (i > lastIndex) 
                     writer.Write(text[lastIndex..i]);

                  switch (decodedChar)
                  {
                     case '<':
                        writer.Write("&lt;");
                        break;
                     case '>':
                        writer.Write("&gt;");
                        break;
                     case '"':
                        writer.Write("&quot;");
                        break;
                     case '&':
                        writer.Write("&amp;");
                        break;
                     default:
                        decoded[0] = decodedChar;
                        writer.Write(decoded);
                        break;
                  }
                  
                  i += consumed - 1;
                  lastIndex = i + 1;
                  continue;
               }
            }

            // Normal encoding logic for non-entity characters
            ReadOnlySpan<char> entity = c switch
            {
               '<' => "&lt;",
               '>' => "&gt;",
               '&' => "&amp;",
               '"' => "&quot;",
               '\'' when encodeApostrophe => "&#39;",
               _ => []
            };

            if (!entity.IsEmpty)
            {
               if (i > lastIndex) writer.Write(text[lastIndex..i]);
               writer.Write(entity);
               
               lastIndex = i + 1;
            }
         }

         if (lastIndex < text.Length) 
            writer.Write(text[lastIndex..]);
      }
      
      public void WriteDecodedEntities(scoped ReadOnlySpan<char> text)
      {
         if (text.IsEmpty) return;

         var lastIndex = 0;
         Span<char> singleCharSpan = stackalloc char[1];

         for (var i = 0; i < text.Length; i++)
         {
            if (text[i] != '&') continue;

            if (i > lastIndex)
            {
               writer.Write(text[lastIndex..i]);
            }

            var decodedChar = SpanUtils.TryParseEntity(text[i..], out var entityLength);
            if (entityLength > 0)
            {
               singleCharSpan[0] = decodedChar;
               writer.Write(singleCharSpan);
         
               i += entityLength - 1;
            }
            else
            {
               writer.Write("&");
            }

            lastIndex = i + 1;
         }

         if (lastIndex < text.Length)
         {
            writer.Write(text[lastIndex..]);
         }
      }
   }
}