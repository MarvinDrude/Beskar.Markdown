using Beskar.Markdown.Rendering.Html.Blocks;
using Beskar.Markdown.Rendering.Html.Inlines;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;

namespace Beskar.Markdown.Rendering;

public sealed class RenderOptions
{
   private const int _builtInNodeTypeCount = (int)NodeType.Autolink + 1;
   
   public ReadOnlySpan<INodeRenderer> NodeRenderers => _nodeRenderer;
    
   public bool PerserveSoftBreaks { get; set; } = true;
   
   public Func<ReadOnlySpan<char>, string>? SanitizerFunc { get; set; }
   
   private readonly INodeRenderer[] _nodeRenderer;
   private readonly INodeRenderer?[] _nodeRendererLookup = new INodeRenderer?[_builtInNodeTypeCount];
   private readonly Dictionary<int, INodeRenderer> _customNodeRendererLookup = [];

   private RenderOptions(IEnumerable<INodeRenderer> nodeRenderers)
   {
      _nodeRenderer = nodeRenderers.ToArray();
      
      for (var i = 0; i < _nodeRenderer.Length; i++)
      {
         var renderer = _nodeRenderer[i];
         var targetType = renderer.TargetTypeValue;
         if ((uint)targetType < _nodeRendererLookup.Length)
         {
            if (_nodeRendererLookup[targetType] != null)
            {
               throw new InvalidOperationException($"Duplicate renderer for type {targetType}");
            }

            _nodeRendererLookup[targetType] = renderer;
         }
         else if (!_customNodeRendererLookup.TryAdd(targetType, renderer))
         {
            throw new InvalidOperationException($"Duplicate renderer for type {targetType}");
         }
      }
   }
    
   public INodeRenderer? GetRenderer(int type)
   {
      if ((uint)type < _nodeRendererLookup.Length)
      {
         return _nodeRendererLookup[type];
      }

      return _customNodeRendererLookup.GetValueOrDefault(type);
   }

   public static RenderOptions HtmlDefault => new([
      // Default block renderers
      new HtmlDocumentRenderer(),
      new HtmlHeaderRenderer(),
      new HtmlBlockQuoteRenderer(),
      new HtmlBlockRenderer(),
      new HtmlCodeBlockRenderer(),
      new HtmlIndentedCodeBlockRenderer(),
      new HtmlListRenderer(),
      new HtmlListItemRenderer(),
      new HtmlParagraphRenderer(),
      new HtmlThematicBreakRenderer(),
      new HtmlTableRenderer(),
      new HtmlTableHeaderRenderer(),
      new HtmlTableBodyRenderer(),
      new HtmlTableRowRenderer(),
      new HtmlTableCellRenderer(),
      // Default inline renderers
      new HtmlTextRenderer(),
      new HtmlAutolinkRenderer(),
      new HtmlEmphasisRenderer(),
      new HtmlImageRenderer(),
      new HtmlInlineCodeRenderer(),
      new HtmlInlineHtmlRenderer(),
      new HtmlLineBreakRenderer(),
      new HtmlSoftBreakRenderer(),
      new HtmlStrikeThroughRenderer(),
      new HtmlStrongEmphasisRenderer(),
      new HtmlLinkRenderer()
   ]);
}
