namespace Beskar.Markdown.Utils;

public static class SpanUtils
{
   public static char TryParseEntity(ReadOnlySpan<char> span, out int consumed)
   {
      consumed = 0;
      if (span.Length < 3) 
         return '\0';

      var semiColonIndex = span.IndexOf(';');
      if (semiColonIndex is -1 or > 10) 
         return '\0';

      var content = span[1..semiColonIndex];

      // Handle Numeric Entities (&#10; or &#x0A;)
      if (content.Length > 1 && content[0] == '#')
      {
         var isHex = content.Length > 2 && (content[1] == 'x' || content[1] == 'X');
         var numberSpan = isHex ? content[2..] : content[1..];
      
         var style = isHex 
            ? System.Globalization.NumberStyles.HexNumber 
            : System.Globalization.NumberStyles.Integer;
      
         if (uint.TryParse(numberSpan, style, null, out var codePoint))
         {
            consumed = semiColonIndex + 1;
            return (char)codePoint; 
         }
      }
   
      var named = content switch
      {
         "amp"  => '&',
         "lt"   => '<',
         "gt"   => '>',
         "quot" => '"',
         "nbsp" => (char)160,
         _      => '\0'
      };

      if (named != 0)
      {
         consumed = semiColonIndex + 1;
         return named;
      }

      return '\0';
   }

   public static int CountTrailingSpaces(ReadOnlySpan<char> span)
   {
      var count = 0;
      for (var i = span.Length - 1; i >= 0; i--)
      {
         var c = span[i];
         if (c is ' ' or '\t')
         {
            count++;
         }
         else
         {
            break;
         }
      }

      return count;
   }

   public static bool IsHardBreak(ReadOnlySpan<char> span)
   {
      if (span.Length == 0) return false;
      
      var last = span[^1];
      if (last == '\t') return true;
      if (last != ' ') return false;
      
      return span.Length >= 2 && span[^2] == ' ';
   }
}