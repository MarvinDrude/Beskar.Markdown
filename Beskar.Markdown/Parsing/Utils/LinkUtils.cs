using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Utils;

public static class LinkUtils
{
   public static ReadOnlySpan<char> NormalizeLabel(ReadOnlySpan<char> span, Span<char> destination)
   {
      if (span.IsEmpty)
         return ReadOnlySpan<char>.Empty;

      var trimmed = span.Trim();
      if (trimmed.IsEmpty)
         return ReadOnlySpan<char>.Empty;

      using var intermediateOwner = trimmed.Length <= 32
         ? new SpanOwner<char>(stackalloc char[trimmed.Length])
         : new SpanOwner<char>(trimmed.Length);

      var tempBuffer = intermediateOwner.Span;
      var collapsedLen = CollapseWhitespace(trimmed, tempBuffer);
      var collapsedResult = tempBuffer[..collapsedLen];

      var status = FoldCase(collapsedResult, destination);
      if (status < 0)
      {
         return ReadOnlySpan<char>.Empty;
      }

      return destination[..status];
   }

   private static int CollapseWhitespace(ReadOnlySpan<char> source, Span<char> destination)
   {
      var len = 0;
      var lastWasSpace = false;

      foreach (var c in source)
      {
         if (char.IsWhiteSpace(c))
         {
            if (lastWasSpace) continue;

            destination[len++] = ' ';
            lastWasSpace = true;
         }
         else
         {
            destination[len++] = c;
            lastWasSpace = false;
         }
      }

      return len;
   }

   public static string NormalizeLabel(ReadOnlySpan<char> span)
   {
      if (span.IsEmpty)
         return string.Empty;

      // case-folding ex ß to ss
      var maxPossibleLen = span.Length * 3;
      using var owner = maxPossibleLen < 512
         ? new SpanOwner<char>(stackalloc char[maxPossibleLen])
         : new SpanOwner<char>(maxPossibleLen);
      var buffer = owner.Span;

      var normalized = NormalizeLabel(span, buffer);

      return new string(normalized);
   }

   public static bool IsAsciiPunctuation(char c)
   {
      return c is >= '!' and <= '/'
         or >= ':' and <= '@'
         or >= '[' and <= '`'
         or >= '{' and <= '~';
   }

   public static bool TryResolveReference<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> label,
      out int nodeIndex)
   {
      if (label.IsEmpty)
      {
         nodeIndex = -1;
         return false;
      }

      // case-folding ex ß to ss
      var maxPossibleLen = label.Length * 3;
      using var owner = maxPossibleLen < 512
         ? new SpanOwner<char>(stackalloc char[maxPossibleLen])
         : new SpanOwner<char>(maxPossibleLen);
      var buffer = owner.Span;

      var normalized = NormalizeLabel(label, buffer);

      return context.ReferenceDefinitions
         .GetAlternateLookup<ReadOnlySpan<char>>()
         .TryGetValue(normalized, out nodeIndex);
   }

   private static int FoldCase(ReadOnlySpan<char> source, Span<char> destination)
   {
      var len = 0;

      foreach (var c in source)
      {
         if (char.IsAscii(c))
         {
            if (len >= destination.Length) return -1;

            // ASCII bitwise math is maybe? faster than char.ToLowerInvariant
            destination[len++] = (char)(c | 0x20);
            continue;
         }

         switch (c)
         {
            case '\u00DF': // ß
            case '\u1E9E': // ẞ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = 's';
               destination[len++] = 's';
               break;

            case '\u0130': // İ (Capital I with dot above)
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = 'i';
               destination[len++] = '\u0307'; // Combining dot above
               break;

            case '\u0149': // ŉ (Deprecated Afrikaans apostrophe n)
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u02BC'; // Modifier letter apostrophe
               destination[len++] = 'n';
               break;

            case '\u01F0': // ǰ (Latin small j with caron)
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = 'j';
               destination[len++] = '\u030C'; // Combining caron
               break;

            // --- 2. Latin Ligatures ---
            case '\uFB00': // ﬀ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = 'f'; destination[len++] = 'f'; break;
            case '\uFB01': // ﬁ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = 'f'; destination[len++] = 'i'; break;
            case '\uFB02': // ﬂ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = 'f'; destination[len++] = 'l'; break;
            case '\uFB05': // ﬅ
            case '\uFB06': // ﬆ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = 's'; destination[len++] = 't'; break;

            case '\uFB03': // ﬃ (Expands to 3 characters)
               if (len + 2 >= destination.Length) return -1;
               destination[len++] = 'f'; destination[len++] = 'f'; destination[len++] = 'i'; 
               break;
            case '\uFB04': // ﬄ (Expands to 3 characters)
               if (len + 2 >= destination.Length) return -1;
               destination[len++] = 'f'; destination[len++] = 'f'; destination[len++] = 'l'; 
               break;

            case '\uFB13': // ﬓ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u0574'; destination[len++] = '\u0576'; break;
            case '\uFB14': // ﬔ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u0574'; destination[len++] = '\u0565'; break;
            case '\uFB15': // ﬕ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u0574'; destination[len++] = '\u056B'; break;
            case '\uFB16': // ﬖ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u057E'; destination[len++] = '\u0576'; break;
            case '\uFB17': // ﬗ
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u0574'; destination[len++] = '\u056D'; break;

            case '\u0390': // ΐ
               if (len + 2 >= destination.Length) return -1;
               destination[len++] = '\u03B9'; destination[len++] = '\u0308'; destination[len++] = '\u0301'; 
               break;
            case '\u03B0': // ΰ
               if (len + 2 >= destination.Length) return -1;
               destination[len++] = '\u03C5'; destination[len++] = '\u0308'; destination[len++] = '\u0301'; 
               break;

            // Alpha with iota (ᾀ...ᾏ)
            case >= '\u1F80' and <= '\u1F8F': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = (char)('\u1F00' + (c & 0x07)); 
               destination[len++] = '\u03B9';
               break;
               
            // Eta with iota (ᾐ...ᾟ)
            case >= '\u1F90' and <= '\u1F9F': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = (char)('\u1F20' + (c & 0x07)); 
               destination[len++] = '\u03B9';
               break;
               
            // Omega with iota (ᾠ...ᾯ)
            case >= '\u1FA0' and <= '\u1FAF': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = (char)('\u1F60' + (c & 0x07)); 
               destination[len++] = '\u03B9';
               break;

            // Assorted Greek Alpha/Eta/Omega combinations
            case '\u1FB2': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1F70'; destination[len++] = '\u03B9'; break;
            case '\u1FB3': case '\u1FBC': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u03B1'; destination[len++] = '\u03B9'; break;
            case '\u1FB4': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1F71'; destination[len++] = '\u03B9'; break;
            case '\u1FB7': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1FB6'; destination[len++] = '\u03B9'; break;

            case '\u1FC2': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1F74'; destination[len++] = '\u03B9'; break;
            case '\u1FC3': case '\u1FCC': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u03B7'; destination[len++] = '\u03B9'; break;
            case '\u1FC4': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1F75'; destination[len++] = '\u03B9'; break;
            case '\u1FC7': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1FC6'; destination[len++] = '\u03B9'; break;

            case '\u1FF2': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1F7C'; destination[len++] = '\u03B9'; break;
            case '\u1FF3': case '\u1FFC': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u03C9'; destination[len++] = '\u03B9'; break;
            case '\u1FF4': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1F7D'; destination[len++] = '\u03B9'; break;
            case '\u1FF7': 
               if (len + 1 >= destination.Length) return -1;
               destination[len++] = '\u1FF6'; destination[len++] = '\u03B9'; break;

            default:
               if (len >= destination.Length) return -1;
               destination[len++] = char.ToLowerInvariant(c);
               break;
         }
      }

      return len;
   }
}