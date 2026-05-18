using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlHeaderRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Header;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText,
      ref TextWriterIndentSlim writer,
      in MarkdownNode current,
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      if (options.EnableSluggableHeaders)
      {
         writer.WriteInterpolated($"<h{current.HeadingLevel} id=\"");
         WriteSlug(ref writer, current, nodes, rawText);
         writer.Write("\">");
      }
      else
      {
         writer.WriteInterpolated($"<h{current.HeadingLevel}>");
      }

      current.RenderChildren(context, rawText, nodes, ref writer, options);

      if (options.AddBlockNewLines)
      {
         writer.WriteLineInterpolated($"</h{current.HeadingLevel}>");
      }
      else
      {
         writer.WriteInterpolated($"</h{current.HeadingLevel}>");
      }
   }

   private static void WriteSlug(
      ref TextWriterIndentSlim writer, in MarkdownNode header,
      ReadOnlySpan<MarkdownNode> nodes, ReadOnlySpan<char> rawText)
   {
      var hasWrittenContent = false;
      var pendingHyphen = false;

      WriteSlugRecursive(
         header.FirstChildIndex, ref writer, nodes, rawText, 
         ref hasWrittenContent, ref pendingHyphen);

      if (!hasWrittenContent)
      {
         ReadOnlySpan<char> fallback = "section";
         writer.Write(fallback);
      }
   }

   private static void WriteSlugRecursive(
      int childIdx, ref TextWriterIndentSlim writer,
      ReadOnlySpan<MarkdownNode> nodes, ReadOnlySpan<char> rawText,
      ref bool hasWrittenContent, ref bool pendingHyphen)
   {
      Span<char> buffer = stackalloc char[1];

      while (childIdx != -1)
      {
         ref readonly var child = ref nodes[childIdx];
         
         if (child.Type is NodeType.Text)
         {
            var text = rawText.Slice(
               child.TextSpan.Start, child.TextSpan.Length);

            foreach (var c in text)
            {
               if (char.IsLetterOrDigit(c))
               {
                  if (pendingHyphen)
                  {
                     buffer[0] = '-';
                     writer.Write(buffer);
                     pendingHyphen = false;
                  }

                  buffer[0] = char.ToLowerInvariant(c);
                  writer.Write(buffer);
                  
                  hasWrittenContent = true;
               }
               else if (hasWrittenContent && (c is ' ' or '-' or '_'))
               {
                  pendingHyphen = true;
               }
            }
         }

         if (child.FirstChildIndex != -1)
         {
            WriteSlugRecursive(
               child.FirstChildIndex, ref writer, nodes, rawText, 
               ref hasWrittenContent, ref pendingHyphen);
         }

         childIdx = child.NextSiblingIndex;
      }
   }
}