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

   public int VirtualSpaces { get; private set; }
   public int Column { get; private set; }

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
      Column = AdvanceColumn(RawLine[..length], Column);
      RawLine = RawLine[length..];
      
      GlobalOffset += length;
      VirtualSpaces = 0;
      Recalculate();
   }

   public void SliceIndentation(int amount)
   {
      if (amount <= 0) return;

      var physicalToSlice = 0;
      var consumedColumns = 0;

      if (VirtualSpaces > 0)
      {
         var toTake = Math.Min(amount, VirtualSpaces);
         VirtualSpaces -= toTake;
         amount -= toTake;
         consumedColumns += toTake;
      }

      while (amount > 0 && physicalToSlice < RawLine.Length)
      {
         var c = RawLine[physicalToSlice];
         var currentAmount = 0;
         
         if (c == ' ')
         {
            currentAmount = 1;
            physicalToSlice++;
         }
         else if (c == '\t')
         {
            var tabSpaces = 4 - ((Column + consumedColumns) % 4);
            currentAmount = tabSpaces;
            physicalToSlice++;
         }
         else
         {
            break;
         }

         if (currentAmount > amount)
         {
            VirtualSpaces = currentAmount - amount;
            consumedColumns += amount;
            amount = 0;
         }
         else
         {
            amount -= currentAmount;
            consumedColumns += currentAmount;
         }
      }

      if (physicalToSlice > 0)
      {
         RawLine = RawLine[physicalToSlice..];
         GlobalOffset += physicalToSlice;
      }

      Column += consumedColumns;
      
      Recalculate();
   }

   public void ConsumeRest()
   {
      if (RawLine.IsEmpty) return;

      GlobalOffset += RawLine.Length;
      RawLine = default;
      Column = 0;
      VirtualSpaces = 0;
      LeadingSpaces = 0;
      FirstNonSpaceIndex = -1;
      FirstChar = '\0';
      IsBlank = true;
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private void Recalculate()
   {
      LeadingSpaces = VirtualSpaces;
      FirstNonSpaceIndex = -1;
      FirstChar = '\0';
      IsBlank = true;
      var column = Column + VirtualSpaces;

      for (var e = 0; e < RawLine.Length; e++)
      {
         var c = RawLine[e];

         switch (c)
         {
            case ' ':
               LeadingSpaces++;
               column++;
               break;
            case '\t':
               var tabSpaces = 4 - (column % 4);
               LeadingSpaces += tabSpaces;
               column += tabSpaces;
               break;
            default:
               FirstNonSpaceIndex = e;
               FirstChar = c;
               IsBlank = false;
               return;
         }
      }
   }

   private static int AdvanceColumn(ReadOnlySpan<char> text, int column)
   {
      for (var i = 0; i < text.Length; i++)
      {
         column += text[i] == '\t'
            ? 4 - (column % 4)
            : 1;
      }

      return column;
   }
}
