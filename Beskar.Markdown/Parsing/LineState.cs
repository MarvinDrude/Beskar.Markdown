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

   private int _virtualSpaces;
   public int VirtualSpaces => _virtualSpaces;

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
      _virtualSpaces = 0;
      Recalculate();
   }

   public void SliceIndentation(int amount)
   {
      if (amount <= 0) return;

      var physicalToSlice = 0;

      if (_virtualSpaces > 0)
      {
         var toTake = Math.Min(amount, _virtualSpaces);
         _virtualSpaces -= toTake;
         amount -= toTake;
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
            var tabSpaces = 4 - ((LeadingSpaces - GetRemainingLeadingSpaces()) % 4);
            currentAmount = tabSpaces;
            physicalToSlice++;
         }
         else
         {
            break;
         }

         if (currentAmount > amount)
         {
            _virtualSpaces = currentAmount - amount;
            amount = 0;
         }
         else
         {
            amount -= currentAmount;
         }
      }

      if (physicalToSlice > 0)
      {
         RawLine = RawLine[physicalToSlice..];
         GlobalOffset += physicalToSlice;
      }
      
      Recalculate();
   }

   private int GetRemainingLeadingSpaces()
   {
      var spaces = 0;
      
      for (var i = 0; i < RawLine.Length; i++)
      {
         var c = RawLine[i];
         if (c == ' ') spaces++;
         else if (c == '\t') spaces += 4 - (spaces % 4);
         else break;
      }
      
      return spaces;
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
      LeadingSpaces = _virtualSpaces;
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
               LeadingSpaces += 4 - (LeadingSpaces % 4);
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
