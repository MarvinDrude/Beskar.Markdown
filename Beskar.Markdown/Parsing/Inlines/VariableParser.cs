using System;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class VariableParser : IInlineParser
{
   public int Priority => 18_000;
   public int SupportedTypeValue => (int)NodeType.Variable;

   public char TriggerChar => '{';
   public char TriggerAltChar => '{';

   public bool TryMatch<TData>(
      ref InlineState<TData> state,
      int parentIndex,
      ref BufferWriter<MarkdownNode> writer,
      scoped ref InlineParser<TData> parser,
      ParserOptions options)
   {
      if (!options.EnableVariables)
      {
         return false;
      }

      var text = state.RemainingText;
      if (text.Length < 4 || text[0] != '{' || text[1] != '{')
      {
         return false;
      }

      var rawText = state.RawText;
      var rawBase = state.GlobalOffset;
      var scanBound = state.BlockEnd;

      var closeIndex = -1;
      var scanIndex = 2;

      while (rawBase + scanIndex + 1 < scanBound)
      {
         var c = rawText[rawBase + scanIndex];
         if (c is '\n' or '\r')
         {
            break;
         }

         if (c == '}' && rawText[rawBase + scanIndex + 1] == '}')
         {
            closeIndex = scanIndex;
            break;
         }

         scanIndex++;
      }

      if (closeIndex == -1)
      {
         return false;
      }

      var innerContent = rawText.Slice(rawBase + 2, closeIndex - 2);
      if (innerContent.IsEmpty || innerContent.IsWhiteSpace())
      {
         return false;
      }

      var format = VariableFormat.Text;
      var trimmedInner = innerContent.Trim();
      var relativeStart = innerContent.IndexOf(trimmedInner);
      var innerOffset = rawBase + 2 + relativeStart;

      ReadOnlySpan<char> nameSpan;

      var doubleColonIdx = trimmedInner.LastIndexOf("::", StringComparison.Ordinal);
      if (doubleColonIdx >= 0)
      {
         var mod = trimmedInner[(doubleColonIdx + 2)..].Trim();
         if (mod.Equals("html", StringComparison.OrdinalIgnoreCase))
         {
            format = VariableFormat.Html;
            nameSpan = trimmedInner[..doubleColonIdx].Trim();
         }
         else
         {
            nameSpan = trimmedInner;
         }
      }
      else
      {
         var colonIdx = trimmedInner.LastIndexOf(':');
         if (colonIdx >= 0)
         {
            var mod = trimmedInner[(colonIdx + 1)..].Trim();
            if (mod.Equals("md", StringComparison.OrdinalIgnoreCase) ||
                mod.Equals("markdown", StringComparison.OrdinalIgnoreCase))
            {
               format = VariableFormat.Markdown;
               nameSpan = trimmedInner[..colonIdx].Trim();
            }
            else if (mod.Equals("html", StringComparison.OrdinalIgnoreCase))
            {
               format = VariableFormat.Html;
               nameSpan = trimmedInner[..colonIdx].Trim();
            }
            else if (mod.Equals("text", StringComparison.OrdinalIgnoreCase) ||
                     mod.Equals("plain", StringComparison.OrdinalIgnoreCase))
            {
               format = VariableFormat.Text;
               nameSpan = trimmedInner[..colonIdx].Trim();
            }
            else
            {
               nameSpan = trimmedInner;
            }
         }
         else
         {
            nameSpan = trimmedInner;
         }
      }

      nameSpan = nameSpan.Trim("{} \t".AsSpan());
      if (nameSpan.IsEmpty)
      {
         return false;
      }

      var nameOffsetInInner = innerContent.IndexOf(nameSpan);
      var nameStart = rawBase + 2 + nameOffsetInInner;
      var nameLength = nameSpan.Length;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode
      {
         Type = NodeType.Variable,
         TextSpan = new TextSpan(nameStart, nameLength),
         VariableFormat = format,
         FirstChildIndex = -1,
         LastChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      state.Advance(closeIndex + 2);
      return true;
   }
}
