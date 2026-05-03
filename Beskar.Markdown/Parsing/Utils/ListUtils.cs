namespace Beskar.Markdown.Parsing.Utils;

public static class ListUtils
{
   public static bool IsListMarker(ref LineState state, out char listChar, out int length)
   {
      listChar = '\0';
      length = 0;

      if (state.IsBlank) return false;
      
      var raw = state.RawLine[state.FirstNonSpaceIndex..];
      if (raw.Length == 0) return false;

      var c = raw[0];
      
      if (c is '-' or '*' or '+')
      {
         if (raw.Length == 1 || raw[1] == ' ' || raw[1] == '\t')
         {
            listChar = c;
            length = state.FirstNonSpaceIndex + 1; 
            
            return true;
         }
      }

      // check ordered
      if (!char.IsDigit(c)) return false;
      
      var i = 0;
      while (i < raw.Length && char.IsDigit(raw[i]))
      {
         i++;
      }

      if (i >= raw.Length || (raw[i] != '.' && raw[i] != ')')) 
         return false;

      if (i + 1 != raw.Length && raw[i + 1] != ' ' && raw[i + 1] != '\t') 
         return false;
      
      listChar = raw[i];
      length = state.FirstNonSpaceIndex + i + 1;
      
      return true;

   }
}