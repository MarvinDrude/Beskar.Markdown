using Beskar.Markdown.Parsing.Models;

namespace Beskar.Markdown.Parsing.Utils;

public static class FrontMatterUtils
{
   public static int ParseFrontMatter<TContext>(ref ReadOnlySpan<char> rawText, MarkdownContext<TContext> context)
   {
      if (rawText.Length < 3 || !rawText.StartsWith("---"))
      {
         return 0;
      }

      var firstNewline = rawText.IndexOf('\n');
      if (firstNewline == -1) return 0;
   
      var firstLine = rawText[..firstNewline].TrimEnd("\r ");
      if (firstLine.Length != 3) return 0;

      var currentOffset = firstNewline + 1;
      var remainingText = rawText[currentOffset..];

      var frontMatterContentEnd = -1;
      var searchSpan = remainingText;
      var tempOffset = 0;

      while (!searchSpan.IsEmpty)
      {
         var nextNewline = searchSpan.IndexOf('\n');
         var lineLength = nextNewline == -1 ? searchSpan.Length : nextNewline;
         var line = searchSpan[..lineLength];
      
         if (line.TrimEnd("\r ") is "---")
         {
            frontMatterContentEnd = tempOffset;
            currentOffset += tempOffset + (nextNewline == -1 ? searchSpan.Length : nextNewline + 1);
            break;
         }
      
         var advance = nextNewline == -1 ? searchSpan.Length : nextNewline + 1;
         searchSpan = searchSpan[advance..];
         tempOffset += advance;
      }

      if (frontMatterContentEnd == -1) return 0;

      var frontMatterSpan = remainingText[..frontMatterContentEnd];
      foreach (var line in frontMatterSpan.EnumerateLines())
      {
         var trimmedLine = line.Trim();
         if (trimmedLine.IsEmpty) continue;

         var colonIndex = trimmedLine.IndexOf(':');
         if (colonIndex != -1)
         {
            var key = trimmedLine[..colonIndex].TrimEnd().ToString();
            var value = trimmedLine[(colonIndex + 1)..].TrimStart().ToString();
            context.FrontMatter[key] = value;
         }
      }

      rawText = rawText[currentOffset..];
      return currentOffset;
   }
}
