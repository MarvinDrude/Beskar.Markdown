namespace Beskar.Markdown.Parsing;

public ref struct LineIterator(ReadOnlySpan<char> rawText)
{
   private ReadOnlySpan<char> _rawText = rawText;
   private int _consumedOffset = 0;

   public bool TryMoveNext(out LineState state)
   {
      if (_rawText.IsEmpty)
      {
         state = default;
         return false;
      }

      var index = _rawText.IndexOf('\n');
      int lineLengthWithSeparator;
      ReadOnlySpan<char> line;

      if (index >= 0)
      {
         line = _rawText[..index];
         lineLengthWithSeparator = index + 1;
         
         if (line.Length > 0 && line[^1] == '\r')
         {
            line = line[..^1];
         }
      }
      else
      {
         line = _rawText;
         lineLengthWithSeparator = _rawText.Length;
      }

      state = new LineState(line, _consumedOffset);
      
      _rawText = _rawText[lineLengthWithSeparator..];
      _consumedOffset += lineLengthWithSeparator;
      
      return true;
   }
}