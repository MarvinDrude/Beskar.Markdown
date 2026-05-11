namespace Beskar.Markdown.Parsing.Utils;

public static class ListUtils
{
   public static bool IsListMarker<TData>(ref LineState<TData> state, 
      out char listChar, out int length, out int orderedNumber)
   {
      orderedNumber = -1;
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
      orderedNumber = 0;
      
      var i = 0;
      while (i < raw.Length && char.IsDigit(raw[i])
             && byte.TryParse(raw.Slice(i, 1), out var digit))
      {
         orderedNumber = orderedNumber * 10 + digit;
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