using System.Runtime.InteropServices;
using Beskar.Markdown.Parsing.Models;

namespace Beskar.Markdown.Parsing;

[StructLayout(LayoutKind.Auto)]
public ref struct InlineState<TData>(
   MarkdownContext<TData> context,
   ReadOnlySpan<char> rawText,
   ReadOnlySpan<char> remainingText, 
   int globalOffset)
{
   public MarkdownContext<TData> Context = context;
   public ReadOnlySpan<char> RawText = rawText;

   public ReadOnlySpan<char> RemainingText = remainingText;
   public int GlobalOffset = globalOffset;

   public int BlockEnd = rawText.Length;

   public void Advance(int length)
   {
      if (length >= RemainingText.Length)
      {
         GlobalOffset += length;
         RemainingText = ReadOnlySpan<char>.Empty;
      }
      else
      {
         RemainingText = RemainingText[length..];
         GlobalOffset += length;
      }
   }
   
   public bool HasContentOnNextLine()
   {
      var currentEnd = GlobalOffset + RemainingText.Length;
      if (currentEnd >= RawText.Length)
      {
         return false;
      }

      var remaining = RawText[currentEnd..];
      var offsetToNextLine = 0;

      if (remaining.StartsWith("\r\n"))
      {
         offsetToNextLine = 2;
      }
      else if (remaining.Length > 0 && (remaining[0] == '\n' || remaining[0] == '\r'))
      {
         offsetToNextLine = 1;
      }

      var nextLineStart = currentEnd + offsetToNextLine;
      if (nextLineStart >= RawText.Length)
      {
         return false;
      }

      var nextContent = RawText[nextLineStart..];
   
      var nextLineEnd = nextContent.IndexOfAny('\r', '\n');
      var nextLineSpan = nextLineEnd == -1 ? nextContent : nextContent[..nextLineEnd];

      foreach (var c in nextLineSpan)
      {
         if (c != ' ' && c != '\t')
         {
            return true;
         }
      }

      return false;
   }
}