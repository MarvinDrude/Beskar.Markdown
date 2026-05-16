using System.Buffers;
using System.Text;
using Beskar.Markdown.Parsing.Utils;
using Beskar.Markdown.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Extensions;

public static class TextWriterIndentSlimExtensions
{
   private static readonly SearchValues<char> UrlSearchValues =
      SearchValues.Create(' ', '"', '\'', '<', '>', '[', ']', '\\', '`', '&');

   extension(ref TextWriterIndentSlim writer)
   {
      public void WriteHtmlDecodedAndEncoded(scoped ReadOnlySpan<char> text, bool encodeApostrophe = true)
      {
         if (text.IsEmpty) return;
         var lastIndex = 0;

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

      public void WriteCommonMarkdownUrlEncoded(
         scoped ReadOnlySpan<char> text, bool processEscapes = true, bool multiLine = false)
      {
         if (text.IsEmpty) return;

         var firstIndex = TextWriterIndentSlim.FindFirstUrlEncodingIndex(text);
         if (firstIndex == -1)
         {
            writer.Write(text, multiLine);
            return;
         }

         var lastIndex = 0;
         for (var i = 0; i < text.Length; i++)
         {
            var c = text[i];
            if (processEscapes && c == '\\' && i + 1 < text.Length)
            {
               var next = text[i + 1];
               if (TextWriterIndentSlim.IsEscapable(next))
               {
                  if (i > lastIndex)
                  {
                     writer.Write(text[lastIndex..i]);
                  }

                  writer.Write(text.Slice(i + 1, 1));

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
                  {
                     writer.Write(text[lastIndex..i]);
                  }

                  TextWriterIndentSlim.WriteUrlEncodedChar(ref writer, decodedChar);
                  i += consumed - 1;
                  lastIndex = i + 1;

                  continue;
               }
            }

            ReadOnlySpan<char> encoded = c switch
            {
               ' ' => "%20",
               '"' => "%22",
               '\'' => "%27",
               '<' => "%3C",
               '>' => "%3E",
               '&' => "&amp;",
               '[' => "%5B",
               ']' => "%5D",
               '\\' => "%5C",
               '`' => "%60",
               _ => []
            };

            if (encoded.IsEmpty)
            {
               if (c > 0x7F)
               {
                  if (i > lastIndex)
                  {
                     writer.Write(text[lastIndex..i]);
                  }

                  if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                  {
                     TextWriterIndentSlim.WriteUrlEncodedCodePoint(ref writer, char.ConvertToUtf32(c, text[i + 1]));
                     i++;
                  }
                  else
                  {
                     TextWriterIndentSlim.WriteUrlEncodedChar(ref writer, c);
                  }

                  lastIndex = i + 1;
               }

               continue;
            }

            if (i > lastIndex)
            {
               writer.Write(text[lastIndex..i]);
            }

            writer.Write(encoded);
            lastIndex = i + 1;
         }

         if (lastIndex < text.Length)
         {
            writer.Write(text[lastIndex..]);
         }
      }

      private static int FindFirstUrlEncodingIndex(ReadOnlySpan<char> text)
      {
         var index = text.IndexOfAny(UrlSearchValues);
         if (index != -1)
         {
            return index;
         }

         for (var i = 0; i < text.Length; i++)
         {
            if (text[i] > 0x7F)
            {
               return i;
            }
         }

         return -1;
      }

      private static void WriteUrlEncodedChar(ref TextWriterIndentSlim output, char c)
      {
         TextWriterIndentSlim.WriteUrlEncodedCodePoint(ref output, c);
      }

      private static void WriteUrlEncodedCodePoint(ref TextWriterIndentSlim output, int codePoint)
      {
         Span<byte> utf8 = stackalloc byte[4];
         var byteCount = Encoding.UTF8.GetBytes(char.ConvertFromUtf32(codePoint).AsSpan(), utf8);
         const string hex = "0123456789ABCDEF";
         Span<char> encoded = stackalloc char[12];
         var outputIndex = 0;

         for (var i = 0; i < byteCount; i++)
         {
            var b = utf8[i];
            encoded[outputIndex++] = '%';
            encoded[outputIndex++] = hex[b >> 4];
            encoded[outputIndex++] = hex[b & 0x0F];
         }

         output.Write(encoded[..outputIndex]);
      }

      private static bool IsEscapable(char c)
      {
         return c switch
         {
            '\\' or '`' or '*' or '_' or '{' or '}' or '[' or ']' or
               '(' or ')' or '#' or '+' or '-' or '.' or '!' or '>' or
               '"' or '\'' or '$' or '%' or '&' or ',' or '/' or ':' or
               ';' or '<' or '=' or '?' or '@' or '^' or '|' or '~' => true,
            _ => false
         };
      }
   }
}
