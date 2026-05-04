using System.Runtime.InteropServices;

namespace Beskar.Markdown.Parsing;

[StructLayout(LayoutKind.Auto)]
public ref struct InlineState(
   ReadOnlySpan<char> rawText,
   ReadOnlySpan<char> remainingText, 
   int globalOffset)
{
   public ReadOnlySpan<char> RawText = rawText;
   
   public ReadOnlySpan<char> RemainingText = remainingText;
   public int GlobalOffset = globalOffset;

   public void Advance(int length)
   {
      RemainingText = RemainingText[length..];
      GlobalOffset += length;
   }
}