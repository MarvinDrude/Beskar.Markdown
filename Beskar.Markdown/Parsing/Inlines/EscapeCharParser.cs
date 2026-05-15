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
   
   private readonly LineBreakParser _lineBreakParser = new();
   
   public bool TryMatch<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options)
   {
      var text = state.RemainingText;
      if (text.Length < 2)
      {
         var parent = writer.WrittenSpan[parentIndex];
         if (parent.Type is NodeType.Header || !state.HasContentOnNextLine())
         {
            // Treat as literal text instead of a line break if no further content
            return CreateLiteralNode(ref state, parentIndex, ref writer, ref parser);
         }
         
         return _lineBreakParser.TryMatch(ref state, parentIndex, 
            ref writer, ref parser, options);
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
   
   private static bool CreateLiteralNode<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser)
   {
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Text,
         TextSpan = new TextSpan(state.GlobalOffset, 1),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      state.Advance(1);
      return true;
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
