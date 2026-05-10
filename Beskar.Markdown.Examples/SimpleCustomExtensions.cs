using System.Collections.Immutable;
using Beskar.Markdown.Addons;
using Beskar.Markdown.Builders;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Examples;

public sealed class SimpleCustomExtensions
{
   public static void Run()
   {
      var options = MarkdownOptionBuilder.Create()
         .WithExtension(new EmojiInlineExtension())
         .WithExtension(new RedBlockExtension())
         .WithMaxBlockDepth(16)
         .Build();

      const string markdown = 
         """
         Hello, World! .RandomEmoji.

         +red block+
         inside of `code` inline
         red
         > blockquote

         """;
      var result = BeMarkdown.ToHtml(markdown, options);
      Console.WriteLine(result);
   }
}

public sealed class EmojiInlineExtension : BaseInlineExtension
{
   private const int _targetTypeValue = BeMarkdown.BuiltInNodeTypeValueOffset + 4;
   private static readonly ImmutableArray<string> _emojis = ImmutableArray.CreateRange([
      "😀", "🎉", "🚀", "🌟", "🔥", "🐱", "🍕", "💻", "☕"]);

   public EmojiInlineExtension()
   {
      Parsers = [new EmojiInlineParser()];
      Renderers = [new HtmlEmojiInlineRenderer()];
   }

   private sealed class HtmlEmojiInlineRenderer : INodeRenderer
   {
      public int TargetTypeValue => _targetTypeValue;

      public void Render(
         ReadOnlySpan<char> rawText, 
         ref TextWriterIndentSlim writer, 
         in MarkdownNode current, 
         ReadOnlySpan<MarkdownNode> nodes,
         RenderOptions options)
      {
         writer.Write("<span class=\"emoji\">");
         writer.Write(_emojis[Random.Shared.Next(0, _emojis.Length)]);
         writer.Write("</span>");
      }
   }
   
   private sealed class EmojiInlineParser : IInlineParser
   {
      private const string _identifier = ".RandomEmoji.";
      
      public int Priority => 8_000;
      public int SupportedTypeValue => _targetTypeValue;

      public char TriggerChar => '.';
      public char TriggerAltChar => '.';

      public bool TryMatch(
         ref InlineState state, 
         int parentIndex, 
         ref BufferWriter<MarkdownNode> writer, 
         scoped ref InlineParser parser,
         ParserOptions options)
      {
         if (state.RemainingText.Length < _identifier.Length) 
            return false;
         
         if (!state.RemainingText.StartsWith(_identifier))
            return false;

         var nodeIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = (NodeType)SupportedTypeValue,
            TextSpan = new TextSpan(state.GlobalOffset, _identifier.Length),
            
            NextSiblingIndex = -1,
            FirstChildIndex = -1,
            LastChildIndex = -1
         });
         
         parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
         state.Advance(_identifier.Length);
         return true;
      }
   }
}

public sealed class RedBlockExtension : BaseBlockExtension
{
   private const int _targetTypeValue = BeMarkdown.BuiltInNodeTypeValueOffset + 5;
   
   public RedBlockExtension()
   {
      Parsers = [new RedBlockParser()];
      Renderers = [new HtmlRedBlockRenderer()];
   }

   private sealed class HtmlRedBlockRenderer : INodeRenderer
   {
      public int TargetTypeValue => _targetTypeValue;

      public void Render(
         ReadOnlySpan<char> rawText, 
         ref TextWriterIndentSlim writer, 
         in MarkdownNode current, 
         ReadOnlySpan<MarkdownNode> nodes,
         RenderOptions options)
      {
         writer.Write("<div class=\"red-block\">");
         current.RenderChildren(rawText, nodes, ref writer, options);
         writer.Write("</div>");
      }
   }

   private sealed class RedBlockParser : IBlockParser
   {
      private const string _identifier = "+red block+";
      
      public int Priority => 10; // low priority
      public int SupportedTypeValue => _targetTypeValue;
      
      public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
      {
         if (state.IsBlank || state.LeadingSpaces > 0)
         {
            return -1;
         }
         
         if (state.FirstChar != '+' && state.RawLine.Length < _identifier.Length)
         {
            return -1;
         }
         
         if (!state.RawLine.StartsWith(_identifier))
         {
            return -1;
         }
         
         var nodeIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = (NodeType)SupportedTypeValue,
            TextSpan = new TextSpan(-1, 0),
            
            FirstChildIndex = -1,
            NextSiblingIndex = -1,
            LastChildIndex = -1
         });

         state.ConsumeRest();
         return nodeIndex;
      }

      public bool CanContinue(ref MarkdownNode node, ref LineState state, ref BufferWriter<MarkdownNode> writer)
      {
         // simple example only an empty line can stop the block
         if (state.IsBlank)
         {
            return false;
         }
         
         if (node.TextSpan.Start == -1)
         {
            node.TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length);
         }
         else
         {
            var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
            node.TextSpan = node.TextSpan with { Length = newLength };
         }

         return true;
      }
   }
}