using System;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class VariableBlockParser : IBlockParser
{
   public int Priority => 10_065;
   public int SupportedTypeValue => (int)NodeType.Variable;

   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces >= 4)
      {
         return -1;
      }

      var rawLine = state.RawLine;
      var trimmed = rawLine[state.FirstNonSpaceIndex..].TrimEnd();

      if (trimmed.Length < 4 || trimmed[0] != '{' 
            || trimmed[1] != '{' || (trimmed.Length > 2 && trimmed[2] == '{') 
            || !trimmed.EndsWith("}}") || trimmed.EndsWith("}}}"))
      {
         return -1;
      }

      var innerWithoutEnds = trimmed[2..^2];
      var closeIndex = innerWithoutEnds.IndexOf("}}");
      if (closeIndex != -1)
      {
         return -1;
      }

      if (innerWithoutEnds.IsEmpty || innerWithoutEnds.IsWhiteSpace())
      {
         return -1;
      }

      var format = VariableFormat.Text;
      var trimmedInner = innerWithoutEnds.Trim();

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
         return -1;
      }

      var relativeStart = rawLine.IndexOf(nameSpan);
      var nameStart = state.GlobalOffset + relativeStart;
      var nameLength = nameSpan.Length;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode
      {
         Type = NodeType.Variable,
         TextSpan = new TextSpan(nameStart, nameLength),
         VariableFormat = format,
         VariableIsBlock = 1,
         FirstChildIndex = -1,
         LastChildIndex = -1,
         NextSiblingIndex = -1
      });

      state.ConsumeRest();
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, ref BufferWriter<MarkdownNode> writer)
   {
      return false;
   }
}
