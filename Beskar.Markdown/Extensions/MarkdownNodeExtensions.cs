using System.Runtime.CompilerServices;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Extensions;

public static class MarkdownNodeExtensions
{
   extension(scoped in MarkdownNode node)
   {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      public void RenderChildren<TData>(
         MarkdownContext<TData> context,
         ReadOnlySpan<char> rawText,
         in ReadOnlySpan<MarkdownNode> nodes,
         ref TextWriterIndentSlim writer,
         RenderOptions options)
      {
         var currentChildIndex = node.FirstChildIndex;

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
}