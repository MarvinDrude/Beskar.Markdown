using Beskar.Markdown.Parsing.Blocks;
using Beskar.Markdown.Parsing.Inlines;
using Beskar.Markdown.Parsing.Interfaces;

namespace Beskar.Markdown.Parsing.Models;

public sealed class ParserOptions
{
   public ReadOnlySpan<IBlockParser> BlockParsers => _blockParsers;
   public ReadOnlySpan<IInlineParser> InlineParsers => _inlineParsers;

   private const int _builtInNodeTypeCount = (int)NodeType.Autolink + 1;

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

   public bool ParseFrontMatter { get; init; }
   
   private readonly IBlockParser[] _blockParsers;
   private readonly IBlockParser?[] _blockParserLookup = new IBlockParser?[_builtInNodeTypeCount];
   private readonly Dictionary<int, IBlockParser> _customBlockParserLookup = [];
    
   private readonly IInlineParser[] _inlineParsers;
   private readonly IInlineParser?[] _inlineParserLookup = new IInlineParser?[_builtInNodeTypeCount];
   private readonly Dictionary<int, IInlineParser> _customInlineParserLookup = [];
   
   private readonly IInlineParser[]?[] _triggerMap = new IInlineParser[]?[256];

   public ParserOptions(
      IEnumerable<IBlockParser> blockParsers,
      IEnumerable<IInlineParser> inlineParsers)
   {
      _blockParsers = blockParsers.OrderByDescending(x => x.Priority).ToArray();
      _inlineParsers = inlineParsers.OrderByDescending(x => x.Priority).ToArray();
      
      for (var i = 0; i < _blockParsers.Length; i++)
      {
         var parser = _blockParsers[i];
         var parserType = parser.SupportedTypeValue;
         if ((uint)parserType < _blockParserLookup.Length)
         {
            if (_blockParserLookup[parserType] != null)
            {
               throw new InvalidOperationException($"Duplicate block parser for type {parserType}");
            }

            _blockParserLookup[parserType] = parser;
         }
         else if (!_customBlockParserLookup.TryAdd(parserType, parser))
         {
            throw new InvalidOperationException($"Duplicate block parser for type {parserType}");
         }
      }
      
      var tempTriggers = new List<IInlineParser>?[256];
      
      for (var i = 0; i < _inlineParsers.Length; i++)
      {
         var parser = _inlineParsers[i];
         var parserType = parser.SupportedTypeValue;
         if ((uint)parserType < _inlineParserLookup.Length)
         {
            if (_inlineParserLookup[parserType] != null)
            {
               throw new InvalidOperationException($"Duplicate inline parser for type {parserType}");
            }

            _inlineParserLookup[parserType] = parser;
         }
         else if (!_customInlineParserLookup.TryAdd(parserType, parser))
         {
            throw new InvalidOperationException($"Duplicate inline parser for type {parserType}");
         }

         RegisterTrigger(parser.TriggerChar);
         RegisterTrigger(parser.TriggerAltChar);
         continue;

         void RegisterTrigger(char c)
         {
            if (c == '\0' || c >= 256) return;
            
            var trigs = tempTriggers[c] ??= [];
            if (!trigs.Contains(parser))
            {
               trigs.Add(parser);
            }
         }
      }
      
      for (var i = 0; i < 256; i++)
      {
         if (tempTriggers[i] is { } list)
         {
            _triggerMap[i] = list.OrderByDescending(x => x.Priority).ToArray();
         }
      }
   }
   
   public IInlineParser? GetInlineParser(char c)
   {
      if (c < 256)
      {
         return _triggerMap[c]?[0];
      }
      return null;
   }
   
   public ReadOnlySpan<IInlineParser> GetInlineParsers(char c)
   {
      if (c < 256)
      {
         var parsers = _triggerMap[c];
         return parsers != null ? new ReadOnlySpan<IInlineParser>(parsers) : [];
      }
      
      return default;
   }
   
   public IBlockParser? GetParserForType(int type)
   {
      if ((uint)type < _blockParserLookup.Length)
      {
         return _blockParserLookup[type];
      }

      return _customBlockParserLookup.GetValueOrDefault(type);
   }
    
   public bool IsParserType(int type)
   {
      if ((uint)type < _blockParserLookup.Length)
      {
         return _blockParserLookup[type] != null;
      }

      return _customBlockParserLookup.ContainsKey(type);
   }
   
   public static ParserOptions Default => new([
      // Default block parsers
      new CodeBlockParser(),
      new IndentedCodeBlockParser(),
      new HeaderParser(),
      new ThematicBreakParser(),
      new ListParser(),
      new ListItemParser(),
      new BlockQuoteParser(),
      new HtmlParser(),
      new TableParser(),
      new LinkReferenceDefinitionParser(),
      new ParagraphParser()
   ], [
      // Default inline parsers
      new EscapeCharParser(),
      new InlineCodeParser(),
      new ImageParser(),
      new LinkParser(),
      new EmphasisParser(),
      new LineBreakParser(),
      new StrikethroughParser(),
      new AutolinkParser(),
      new InlineHtmlParser()
   ]);
}
