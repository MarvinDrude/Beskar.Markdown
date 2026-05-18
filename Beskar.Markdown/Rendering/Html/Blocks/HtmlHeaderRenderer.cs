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

      Span<char> slugBuffer = stackalloc char[256];
      Span<char> plainTextBuffer = stackalloc char[256];
      var slugLength = 0;
      var plainTextLength = 0;

      GenerateSlugAndPlainText(
         header.FirstChildIndex, slugBuffer, ref slugLength, plainTextBuffer, ref plainTextLength, 
         nodes, rawText, ref hasWrittenContent, ref pendingHyphen);

      if (!hasWrittenContent)
      {
         ReadOnlySpan<char> fallback = "section";
         fallback.CopyTo(slugBuffer);
         slugLength = fallback.Length;
         fallback.CopyTo(plainTextBuffer);
         plainTextLength = fallback.Length;
      }

      var baseSlug = slugBuffer[..slugLength].ToString();
      var plainText = plainTextBuffer[..plainTextLength].ToString();
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

   private static void GenerateSlugAndPlainText(
      int childIdx, Span<char> slugBuffer, ref int slugLength, 
      Span<char> plainTextBuffer, ref int plainTextLength,
      ReadOnlySpan<MarkdownNode> nodes, ReadOnlySpan<char> rawText,
      ref bool hasWrittenContent, ref bool pendingHyphen)
   {
      while (childIdx != -1)
      {
         ref readonly var child = ref nodes[childIdx];
         
         if (child.Type is NodeType.Text)
         {
            var text = rawText.Slice(
               child.TextSpan.Start, child.TextSpan.Length);

            if (plainTextLength + text.Length <= plainTextBuffer.Length)
            {
               text.CopyTo(plainTextBuffer[plainTextLength..]);
               plainTextLength += text.Length;
            }

            foreach (var c in text)
            {
               if (char.IsLetterOrDigit(c))
               {
                  if (pendingHyphen && slugLength < slugBuffer.Length)
                  {
                     slugBuffer[slugLength++] = '-';
                     pendingHyphen = false;
                  }

                  if (slugLength < slugBuffer.Length)
                  {
                     slugBuffer[slugLength++] = char.ToLowerInvariant(c);
                     hasWrittenContent = true;
                  }
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
               child.FirstChildIndex, slugBuffer, ref slugLength, plainTextBuffer, ref plainTextLength, 
               nodes, rawText, ref hasWrittenContent, ref pendingHyphen);
         }

         childIdx = child.NextSiblingIndex;
      }
   }
}
