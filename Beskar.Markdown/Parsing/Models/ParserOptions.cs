using Beskar.Markdown.Parsing.Blocks;
using Beskar.Markdown.Parsing.Inlines;
using Beskar.Markdown.Parsing.Interfaces;

namespace Beskar.Markdown.Parsing.Models;

public sealed class ParserOptions
{
   public ReadOnlySpan<IBlockParser> BlockParsers => _blockParsers;
   public ReadOnlySpan<IInlineParser> InlineParsers => _inlineParsers;

   public int MaxBlockDepth
   {
      get;
      init
      {
         if (value > 32)
         {
            throw new ArgumentOutOfRangeException(nameof(value), "Block max depth must be less than 32");
         }
         field = value;
      }
   } = 16;
   
   private readonly IBlockParser[] _blockParsers;
   private readonly Dictionary<int, IBlockParser> _blockParserLookup = new();
   
   private readonly IInlineParser[] _inlineParsers;
   private readonly Dictionary<int, IInlineParser> _inlineParserLookup = new();

   public ParserOptions(
      IEnumerable<IBlockParser> blockParsers,
      IEnumerable<IInlineParser> inlineParsers)
   {
      _blockParsers = blockParsers.OrderByDescending(x => x.Priority).ToArray();
      _inlineParsers = inlineParsers.OrderByDescending(x => x.Priority).ToArray();
      
      for (var i = 0; i < _blockParsers.Length; i++)
      {
         var parser = _blockParsers[i];
         if (!_blockParserLookup.TryAdd(parser.SupportedTypeValue, parser))
         {
            throw new InvalidOperationException($"Duplicate block parser for type {parser.SupportedTypeValue}");
         }
      }
      
      for (var i = 0; i < _inlineParsers.Length; i++)
      {
         var parser = _inlineParsers[i];
         if (!_inlineParserLookup.TryAdd(parser.SupportedTypeValue, parser))
         {
            throw new InvalidOperationException($"Duplicate inline parser for type {parser.SupportedTypeValue}");
         }
      }
   }
   
   public IBlockParser? GetParserForType(int type)
      => _blockParserLookup.GetValueOrDefault(type);
   
   public bool IsParserType(int type)
      => _blockParserLookup.ContainsKey(type);
   
   public static ParserOptions Default => new([
      // Default block parsers
      new CodeBlockParser(),
      new HeaderParser(),
      new ListParser(),
      new ListItemParser(),
      new BlockQuoteParser(),
      new ThematicBreakParser(),
      new HtmlParser(),
      new ParagraphParser()
   ], [
      // Default inline parsers
      new BoldParser(),
      new InlineCodeParser(),
      new LinkParser(),
      new ImageParser(),
      new LineBreakParser(),
      new InlineHtmlParser()
   ]);
}