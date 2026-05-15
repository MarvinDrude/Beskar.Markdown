using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Beskar.Markdown.Parsing.Models;

namespace Beskar.Markdown.Parsing;

[DebuggerDisplay("LineState: {RawLine.Length} chars, GlobalOffset: {GlobalOffset}: {RawLine}")]
[StructLayout(LayoutKind.Auto)]
public ref struct LineState<TData>
{
   public MarkdownContext<TData> Context;
   public ReadOnlySpan<char> RawLine;
   public ReadOnlySpan<char> FullText;
   
   public int GlobalOffset;
   
   public int LeadingSpaces;
   public int FirstNonSpaceIndex;
   public char FirstChar;
   public bool IsBlank;

   public LineState(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> fullText,
      ReadOnlySpan<char> rawLine, 
      int globalOffset)
   {
      Context = context;
      
      RawLine = rawLine;
      FullText = fullText;
      GlobalOffset = globalOffset;

      Recalculate();
   }

   public void Slice(int length)
   {
      if (length <= 0) return;
      
      length = Math.Min(length, RawLine.Length);
      RawLine = RawLine[length..];
      
      GlobalOffset += length;
      Recalculate();
   }

   public void ConsumeRest()
   {
      if (RawLine.IsEmpty) return;

      GlobalOffset += RawLine.Length;
      RawLine = default;
      LeadingSpaces = 0;
      FirstNonSpaceIndex = -1;
      FirstChar = '\0';
      IsBlank = true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private void Recalculate()
   {
      LeadingSpaces = 0;
      FirstNonSpaceIndex = -1;
      FirstChar = '\0';
      IsBlank = true;

      for (var e = 0; e < RawLine.Length; e++)
      {
         var c = RawLine[e];

         switch (c)
         {
            case ' ':
               LeadingSpaces++;
               break;
            case '\t':
               LeadingSpaces += 4;
               break;
            default:
               FirstNonSpaceIndex = e;
               FirstChar = c;
               IsBlank = false;
               return;
         }
      }
   }
}
