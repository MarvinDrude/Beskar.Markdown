using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class AutolinkParser : IInlineParser
{
   public int Priority => 6_050;
   public int SupportedTypeValue => (int)NodeType.Autolink;
   
   public char TriggerChar => '<';
   public char TriggerAltChar => '<';
   
   public bool TryMatch(ref InlineState state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser parser,
      ParserOptions options)
   {
      var text = state.RemainingText;
      if (text.Length < 3) return false;

      var closeIdx = -1;
      
      for (var i = 1; i < text.Length; i++)
      {
         var c = text[i];
         if (c == '<' || char.IsWhiteSpace(c)) 
         {
            return false;
         }
         
         if (c == '>')
         {
            closeIdx = i;
            break;
         }
      }

      if (closeIdx == -1) return false;

      var content = text[1..closeIdx];
      var isEmail = IsEmailAutolink(content);
      
      if (isEmail || IsUriAutolink(content))
      {
         var nodeIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = NodeType.Autolink, 
            TextSpan = new TextSpan(state.GlobalOffset + 1, closeIdx - 1),
            FirstChildIndex = -1,
            NextSiblingIndex = -1,
            IsEmail = (byte)(isEmail ? 1 : 0)
         });

         parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
         state.Advance(closeIdx + 1);
         
         return true;
      }

      return false;
   }
   
   private bool IsUriAutolink(ReadOnlySpan<char> content)
   {
      var colonIdx = content.IndexOf(':');
      
      if (colonIdx is < 2 or > 32) return false;
      if (!char.IsAsciiLetter(content[0])) return false;
      
      for (var i = 1; i < colonIdx; i++)
      {
         var c = content[i];
         if (!char.IsAsciiLetterOrDigit(c) 
             && c != '+' && c != '-' && c != '.')
         {
            return false;
         }
      }

      return content.Length > colonIdx + 1;
   }

   private static bool IsEmailAutolink(ReadOnlySpan<char> content)
   {
      var atIdx = content.IndexOf('@');
      return atIdx > 0 && atIdx < content.Length - 1;
   }
}
