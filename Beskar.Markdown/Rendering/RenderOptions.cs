using Beskar.Markdown.Rendering.Html.Blocks;
using Beskar.Markdown.Rendering.Interfaces;

namespace Beskar.Markdown.Rendering;

public sealed class RenderOptions
{
   public ReadOnlySpan<INodeRenderer> NodeRenderers => _nodeRenderer;
   
   private readonly INodeRenderer[] _nodeRenderer;
   private readonly Dictionary<int, INodeRenderer> _nodeRendererLookup = [];
   
   public RenderOptions(IEnumerable<INodeRenderer> nodeRenderers)
   {
      _nodeRenderer = nodeRenderers.ToArray();
      
      for (var i = 0; i < _nodeRenderer.Length; i++)
      {
         var parser = _nodeRenderer[i];
         if (!_nodeRendererLookup.TryAdd(parser.TargetTypeValue, parser))
         {
            throw new InvalidOperationException($"Duplicate block parser for type {parser.TargetTypeValue}");
         }
      }
   }
   
   public INodeRenderer? GetRenderer(int type)
      => _nodeRendererLookup.GetValueOrDefault(type);

   public static RenderOptions HtmlDefault => new([
      new HtmlDocumentRenderer(),
      new HtmlHeaderRenderer(),
      new HtmlBlockQuoteRenderer(),
      new HtmlBlockRenderer(),
      new HtmlCodeBlockRenderer(),
      new HtmlIndentedCodeBlockRenderer()
   ]);
}