using Beskar.Markdown.Parsing.Blocks;
using Beskar.Markdown.Parsing.Interfaces;

namespace Beskar.Markdown.Parsing.Models;

public sealed class ParserOptions
{
   public ReadOnlySpan<IBlockParser> BlockParsers => _blockParsers;

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

   public ParserOptions(IEnumerable<IBlockParser> blockParsers)
   {
      _blockParsers = blockParsers.OrderByDescending(x => x.Priority).ToArray();
      
      for (var i = 0; i < _blockParsers.Length; i++)
      {
         var parser = _blockParsers[i];
         if (!_blockParserLookup.TryAdd(parser.SupportedTypeValue, parser))
         {
            throw new InvalidOperationException($"Duplicate block parser for type {parser.SupportedTypeValue}");
         }
      }
   }
   
   public IBlockParser? GetParserForType(int type)
      => _blockParserLookup.GetValueOrDefault(type);
   
   public bool IsParserType(int type)
      => _blockParserLookup.ContainsKey(type);
   
   public static ParserOptions Default => new([
      new CodeBlockParser(),
      new HeaderParser(),
      new BlockQuoteParser(),
   ]);
}