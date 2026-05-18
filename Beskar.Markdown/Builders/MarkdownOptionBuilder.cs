using Beskar.Markdown.Addons;
using Beskar.Markdown.Addons.Interfaces;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;
using Beskar.Markdown.Rendering.Interfaces;

namespace Beskar.Markdown.Builders;

public sealed class MarkdownOptionBuilder
{
   private static readonly RenderOptions _defaultRenderOptions = RenderOptions.HtmlDefault;
   private static readonly ParserOptions _defaultParserOptions = ParserOptions.Default;
   
   // common
   private readonly List<IMarkdownExtension> _extensions = [];
   
   // render options
   private bool _preserveSoftBreaks = true;
   private bool _enableSluggableHeaders;
   
   private readonly List<INodeRenderer> _nodeRenderers = new (_defaultRenderOptions.NodeRenderers.Length);
   
   private Func<ReadOnlySpan<char>, string>? _sanitizerFunc;
   
   // parser options
   private readonly List<IBlockParser> _blockParsers = new (_defaultParserOptions.BlockParsers.Length);
   private readonly List<IInlineParser> _inlineParsers = new (_defaultParserOptions.InlineParsers.Length);
   
   private int _maxBlockDepth = 16;
   private bool _parseFrontMatter;
   
   private MarkdownOptionBuilder()
   {
      _blockParsers.AddRange(_defaultParserOptions.BlockParsers);
      _inlineParsers.AddRange(_defaultParserOptions.InlineParsers);
      
      _nodeRenderers.AddRange(_defaultRenderOptions.NodeRenderers);
   }
   
   public MarkdownOptionBuilder WithMaxBlockDepth(int maxBlockDepth)
   {
      _maxBlockDepth = maxBlockDepth;
      return this;
   }

   public MarkdownOptionBuilder WithFrontMatter()
   {
      _parseFrontMatter = true;
      return this;
   }

   public MarkdownOptionBuilder WithSanitizer(Func<ReadOnlySpan<char>, string> sanitizerFunc)
   {
      _sanitizerFunc = sanitizerFunc;
      return this;
   }
   
   public MarkdownOptionBuilder WithPreserveSoftBreaks(bool preserveSoftBreaks)
   {
      _preserveSoftBreaks = preserveSoftBreaks;
      return this;
   }

   public MarkdownOptionBuilder WithSluggableHeaders()
   {
      _enableSluggableHeaders = true;
      return this;
   }

   public MarkdownOptionBuilder WithExtension(IMarkdownExtension extension)
   {
      return WithExtensions(extension);
   }
   
   public MarkdownOptionBuilder WithExtensions(params ReadOnlySpan<IMarkdownExtension> extensions)
   {
      _extensions.AddRange(extensions);
      return this;
   }
   
   public MarkdownOptions Build()
   {
      foreach (var extension in _extensions)
      {
         _nodeRenderers.AddRange(extension.Renderers);

         switch (extension)
         {
            case BaseInlineExtension inline:
               _inlineParsers.AddRange(inline.Parsers);
               break;
            case BaseBlockExtension block:
               _blockParsers.AddRange(block.Parsers);
               break;
         }
      }
      
      return new MarkdownOptions()
      {
         RenderOptions = new RenderOptions(_nodeRenderers)
         {
            PreserveSoftBreaks = _preserveSoftBreaks,
            SanitizerFunc = _sanitizerFunc,
            EnableSluggableHeaders = _enableSluggableHeaders
         },
         ParserOptions = new ParserOptions(_blockParsers, _inlineParsers)
         {
            MaxBlockDepth = _maxBlockDepth,
            ParseFrontMatter = _parseFrontMatter
         }
      };
   }

   /// <summary>
   /// Creates a new instance of <see cref="MarkdownOptionBuilder"/> with default options.
   /// </summary>
   public static MarkdownOptionBuilder Create()
   {
      return new MarkdownOptionBuilder();
   }
}