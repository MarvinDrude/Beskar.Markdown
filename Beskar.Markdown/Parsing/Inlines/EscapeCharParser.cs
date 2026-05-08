using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class EscapeCharParser : IInlineParser
{
   public int Priority => 100_000;
   public int SupportedTypeValue => (int)NodeType.Text;

   public char TriggerChar => '\\';
   public char TriggerAltChar => '\\';
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser parser)
   {
      var text = state.RemainingText;
      if (text.Length < 2)
      {
         return false;
      }

      var nextChar = text[1];
      if (IsEscapable(nextChar))
      {
         var nodeIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = NodeType.Text,
            TextSpan = new TextSpan(state.GlobalOffset + 1, 1),
            FirstChildIndex = -1,
            NextSiblingIndex = -1
         });

         parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
         
         state.Advance(2);
         return true;
      }

      return false;
   }
   
   private static bool IsEscapable(char c)
   {
      return c switch
      {
         '\\' or '`' or '*' or '_' or '{' or '}' or '[' or ']' or
            '(' or ')' or '#' or '+' or '-' or '.' or '!' or '>' or
            '"' or '\'' or '$' or '%' or '&' or ',' or '/' or ':' or
            ';' or '<' or '=' or '?' or '@' or '^' or '|' or '~' => true,
         _ => false
      };
   }
}
