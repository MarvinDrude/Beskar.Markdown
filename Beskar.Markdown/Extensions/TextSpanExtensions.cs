using System.Runtime.CompilerServices;
using Beskar.Markdown.Parsing.Models;

namespace Beskar.Markdown.Extensions;

public static class TextSpanExtensions
{
   extension(scoped in TextSpan span)
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public ReadOnlySpan<char> Slice(ReadOnlySpan<char> rawText)
      {
         if (span.Start < 0 || span.Length < 0)
         {
            return [];
         }
         
         return rawText.Slice(span.Start, span.Length);
      }
   }
}