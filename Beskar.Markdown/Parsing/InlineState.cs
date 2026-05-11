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

   public void Advance(int length)
   {
      RemainingText = RemainingText[length..];
      GlobalOffset += length;
   }
}