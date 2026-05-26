using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering;

public readonly ref struct MarkdownRenderer(ReadOnlySpan<char> rawText)
{
   private readonly ReadOnlySpan<char> _rawText = rawText;

   public void Render<TData>(
      MarkdownContext<TData> context, 
      in ReadOnlySpan<MarkdownNode> nodes, 
      RenderOptions options, 
      ref TextWriterIndentSlim writer)
   {
      if (nodes.IsEmpty) 
         return;
      
      var documentNode = nodes[0];
      if (documentNode.Type is not NodeType.Document)
      {
         throw new InvalidOperationException("The first node must be a document node.");
      }

      var renderer = options.GetRenderer((int)NodeType.Document)
         ?? throw new InvalidOperationException("No renderer found for document node.");
      renderer.Render(context, _rawText, ref writer, in documentNode, nodes, options);
   }
   
   public string Render<TData>(
      MarkdownContext<TData> context, 
      in ReadOnlySpan<MarkdownNode> nodes, 
      RenderOptions options)
   {
      if (nodes.IsEmpty) 
         return string.Empty;
      
      var writer = new TextWriterIndentSlim(
         stackalloc char[512], stackalloc char[64]);
      
      try
      {
         Render(context, in nodes, options, ref writer);

         return writer.ToString();
      }
      finally
      {
         writer.Dispose();
      }
   }
}
