namespace Beskar.Markdown.Parsing.Utils;

internal static class HtmlTagUtils
{
   public static bool TryParseCompleteTag(ReadOnlySpan<char> text, out int length)
   {
      return TryParseClosingTag(text, out length) 
         || TryParseOpenTag(text, out length);
   }

   public static bool TryParseClosingTag(ReadOnlySpan<char> text, out int length)
   {
      length = 0;
      if (text.Length < 4 || text[0] != '<' || text[1] != '/' || !char.IsAsciiLetter(text[2]))
      {
         return false;
      }

      var i = 3;
      while (i < text.Length && IsTagNameChar(text[i]))
      {
         i++;
      }

      while (i < text.Length && IsHtmlWhitespace(text[i]))
      {
         i++;
      }

      if (i >= text.Length || text[i] != '>')
      {
         return false;
      }

      length = i + 1;
      return true;
   }

   public static bool TryParseOpenTag(ReadOnlySpan<char> text, out int length)
   {
      length = 0;
      if (text.Length < 3 || text[0] != '<' || !char.IsAsciiLetter(text[1]))
      {
         return false;
      }

      var i = 2;
      while (i < text.Length && IsTagNameChar(text[i]))
      {
         i++;
      }

      if (i >= text.Length)
      {
         return false;
      }

      if (text[i] == '>')
      {
         length = i + 1;
         return true;
      }

      if (text[i] == '/')
      {
         if (i + 1 < text.Length && text[i + 1] == '>')
         {
            length = i + 2;
            return true;
         }

         return false;
      }

      if (!IsHtmlWhitespace(text[i]))
      {
         return false;
      }

      while (true)
      {
         while (i < text.Length && IsHtmlWhitespace(text[i]))
         {
            i++;
         }

         if (i >= text.Length)
         {
            return false;
         }

         if (text[i] == '>')
         {
            length = i + 1;
            return true;
         }

         if (text[i] == '/')
         {
            if (i + 1 < text.Length && text[i + 1] == '>')
            {
               length = i + 2;
               return true;
            }

            return false;
         }

         if (!TryParseAttribute(text, ref i))
         {
            return false;
         }

         if (i < text.Length && text[i] is not ('>' or '/') && !IsHtmlWhitespace(text[i]))
         {
            return false;
         }
      }
   }

   public static bool IsHtmlWhitespace(char c)
   {
      return c is ' ' or '\t' or '\n' or '\r' or '\f';
   }

   private static bool TryParseAttribute(ReadOnlySpan<char> text, ref int index)
   {
      if (index >= text.Length || !IsAttributeNameStart(text[index]))
      {
         return false;
      }

      index++;
      while (index < text.Length && IsAttributeNameChar(text[index]))
      {
         index++;
      }

      var afterName = index;
      while (index < text.Length && IsHtmlWhitespace(text[index]))
      {
         index++;
      }

      if (index >= text.Length || text[index] != '=')
      {
         index = afterName;
         return true;
      }

      index++;
      while (index < text.Length && IsHtmlWhitespace(text[index]))
      {
         index++;
      }

      if (index >= text.Length)
      {
         return false;
      }

      var quote = text[index];
      if (quote is '"' or '\'')
      {
         index++;
         while (index < text.Length && text[index] != quote)
         {
            index++;
         }

         if (index >= text.Length)
         {
            return false;
         }

         index++;
         return true;
      }

      var valueStart = index;
      while (index < text.Length)
      {
         var c = text[index];
         if (IsHtmlWhitespace(c) || c is '>')
         {
            break;
         }

         if (c is '"' or '\'' or '=' or '<' or '`')
         {
            return false;
         }

         index++;
      }

      return index > valueStart;
   }

   private static bool IsTagNameChar(char c)
   {
      return char.IsAsciiLetterOrDigit(c) || c == '-';
   }

   private static bool IsAttributeNameStart(char c)
   {
      return char.IsAsciiLetter(c) || c is '_' or ':';
   }

   private static bool IsAttributeNameChar(char c)
   {
      return char.IsAsciiLetterOrDigit(c) || c is '_' or '.' or ':' or '-';
   }
}
