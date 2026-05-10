using System.Collections.Immutable;
using Beskar.Markdown;
using Beskar.Markdown.Addons;
using Beskar.Markdown.Builders;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Rendering.Interfaces;

Console.WriteLine("Hello, World!");

var options = MarkdownOptionBuilder.Create()
   .WithExtension(new EmojiInlineExtension())
   .WithExtension(new RedBlockExtension())
   .WithMaxBlockDepth(16)
   .Build();

const string markdown = 
   """
   Hello, World! .RandomEmoji.
   
   +red block+
   inside of
   red
   
   """;
var result = BeMarkdown.ToHtml(markdown, options);
Console.WriteLine(result);

return;

public sealed class EmojiInlineExtension : BaseInlineExtension
{
   private readonly ImmutableArray<string> _emojis = ImmutableArray.CreateRange([
      "😀", "🎉", "🚀", "🌟", "🔥", "🐱", "🍕", "💻", "☕"]);

   public EmojiInlineExtension()
   {
      Parsers = [new EmojiInlineParser()];
      Renderers = [new HtmlEmojiInlineRenderer()];
   }

   public sealed class HtmlEmojiInlineRenderer : INodeRenderer
   {
      
   }
   
   public sealed class EmojiInlineParser : IInlineParser
   {
      
   }
}

public sealed class RedBlockExtension : BaseBlockExtension
{
   
}