using System.Runtime.CompilerServices;

namespace Beskar.Markdown.Parsing;

public ref struct LineState
{
   public ReadOnlySpan<char> RawLine;
   
   public int GlobalOffset;
   
   public int LeadingSpaces;
   public int FirstNonSpaceIndex;
   public char FirstChar;
   public bool IsBlank;

   public LineState(ReadOnlySpan<char> rawLine, int globalOffset)
   {
      RawLine = rawLine;
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
