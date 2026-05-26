using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlDocumentRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Document;
   
   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current,
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var currentChildIndex = current.FirstChildIndex;

      while (currentChildIndex != -1)
      {
         var child = nodes[currentChildIndex];
         var renderer = options.GetRenderer((int)child.Type)
            ?? throw new InvalidOperationException($"No renderer found for node type {child.Type}");
         
         renderer.Render(context, rawText, ref writer, child, nodes, options);
         currentChildIndex = child.NextSiblingIndex;
      }
   }
}