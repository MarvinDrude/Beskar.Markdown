using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Inlines;

public sealed class PlainImageRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Image;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      WriteAltText(context, rawText, ref writer, current.FirstChildIndex, nodes, options);
   }

   private static void WriteAltText<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText,
      ref TextWriterIndentSlim writer,
      int childIndex,
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      while (childIndex != -1)
      {
         var child = nodes[childIndex];

         switch (child.Type)
         {
            case NodeType.Text:
            case NodeType.InlineHtml:
            case NodeType.Autolink:
               writer.WriteHtmlDecoded(child.TextSpan.Slice(rawText));
               break;
            case NodeType.InlineCode:
               writer.WriteHtmlDecoded(child.TextSpan.Slice(rawText).Trim());
               break;
            case NodeType.SoftBreak:
            case NodeType.LineBreak:
               writer.Write("\n");
               break;
            default:
               WriteAltText(context, rawText, ref writer, child.FirstChildIndex, nodes, options);
               break;
         }

         childIndex = child.NextSiblingIndex;
      }
   }
}
