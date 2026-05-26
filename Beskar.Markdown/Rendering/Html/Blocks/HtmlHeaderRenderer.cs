using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

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
         WriteUniqueSlug(context, ref writer, current, nodes, rawText);
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

   private static void WriteUniqueSlug<TData>(
      MarkdownContext<TData> context,
      ref TextWriterIndentSlim writer, in MarkdownNode header,
      ReadOnlySpan<MarkdownNode> nodes, ReadOnlySpan<char> rawText)
   {
      var hasWrittenContent = false;
      var pendingHyphen = false;

      var slugWriter = new TextWriterIndentSlim(stackalloc char[128], stackalloc char[1]);
      var plainTextWriter = new TextWriterIndentSlim(stackalloc char[128], stackalloc char[1]);

      try
      {
         GenerateSlugAndPlainText(
            header.FirstChildIndex, ref slugWriter, ref plainTextWriter, 
            nodes, rawText, ref hasWrittenContent, ref pendingHyphen);

         if (!hasWrittenContent)
         {
            slugWriter.Write("section");
            plainTextWriter.Write("section");
         }

         var baseSlug = slugWriter.ToString();
         var plainText = plainTextWriter.ToString();
         
         var uniqueSlug = baseSlug;
         var counter = 1;

         while (context.SlugToPlainText.ContainsKey(uniqueSlug))
         {
            uniqueSlug = $"{baseSlug}-{counter++}";
         }

         context.SlugToPlainText[uniqueSlug] = plainText;
         context.Headers.Add(new HeaderInfo(uniqueSlug, plainText, header.HeadingLevel));
         writer.Write(uniqueSlug);
      }
      finally
      {
         slugWriter.Dispose();
         plainTextWriter.Dispose();
      }
   }

   private static void GenerateSlugAndPlainText(
      int childIdx, ref TextWriterIndentSlim slugWriter, ref TextWriterIndentSlim plainTextWriter,
      ReadOnlySpan<MarkdownNode> nodes, ReadOnlySpan<char> rawText,
      ref bool hasWrittenContent, ref bool pendingHyphen)
   {
      Span<char> charBuffer = stackalloc char[1];
      while (childIdx != -1)
      {
         ref readonly var child = ref nodes[childIdx];
         
         if (child.Type is NodeType.Text or NodeType.InlineCode)
         {
            var text = rawText.Slice(
               child.TextSpan.Start, child.TextSpan.Length);

            plainTextWriter.Write(text);

            foreach (var c in text)
            {
               if (char.IsLetterOrDigit(c))
               {
                  if (pendingHyphen)
                  {
                     charBuffer[0] = '-';
                     slugWriter.Write(charBuffer);
                     pendingHyphen = false;
                  }

                  charBuffer[0] = char.ToLowerInvariant(c);
                  slugWriter.Write(charBuffer);
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
            GenerateSlugAndPlainText(
               child.FirstChildIndex, ref slugWriter, ref plainTextWriter, 
               nodes, rawText, ref hasWrittenContent, ref pendingHyphen);
         }

         childIdx = child.NextSiblingIndex;
      }
   }
}
