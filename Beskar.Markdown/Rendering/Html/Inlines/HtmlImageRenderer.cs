using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlImageRenderer : INodeRenderer
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
      var url = rawText.Slice(current.LinkUrlStart, current.LinkUrlLength);
      writer.Write("<img src=\"");
      writer.WriteCommonMarkdownUrlEncoded(url);
      writer.Write("\" alt=\"");
      WriteAltText(context, rawText, ref writer, current.FirstChildIndex, nodes, options);
      writer.Write("\" ");

      if (current.LinkTitleOffset > 0)
      {
         var startIndex = current.LinkUrlStart + current.LinkUrlLength + current.LinkTitleOffset;
         writer.Write("title=\"");
         writer.WriteHtmlDecodedAndEncoded(rawText.Slice(startIndex, current.LinkTitleLength), encodeApostrophe: false);
         writer.Write("\" ");
      }
      
      writer.Write("/>");
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
               writer.WriteHtmlDecodedAndEncoded(child.TextSpan.Slice(rawText), encodeApostrophe: false);
               break;
            case NodeType.InlineCode:
               writer.WriteHtmlDecodedAndEncoded(child.TextSpan.Slice(rawText).Trim(), encodeApostrophe: false);
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
