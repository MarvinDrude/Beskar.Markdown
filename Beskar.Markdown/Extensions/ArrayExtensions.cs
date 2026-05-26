using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Extensions;

public static class ArrayExtensions
{
   extension(MarkdownNode[] nodes)
   {
      public string CreateDebugString(ReadOnlySpan<char> rawText)
      {
         var writer = new TextWriterIndentSlim(stackalloc char[512], stackalloc char[64]);
         try
         {
            nodes.WriteDebugString(rawText, ref writer, 0);
            return writer.ToString();
         }
         finally
         {
            writer.Dispose();
         }
      }
      
      public void WriteDebugString(ReadOnlySpan<char> rawText, ref TextWriterIndentSlim writer, int nodeIndex)
      {
         if (nodeIndex == -1 || nodeIndex >= nodes.Length) return;
         var node = nodes[nodeIndex];
         
         
         var textSnippet = node.TextSpan.Slice(rawText).ToString()
            .Replace("\r\n", "\\n").Replace("\n", "\\n");

         writer.WriteLineInterpolated($"└─ [{nodeIndex}] {node.Type} (\"{textSnippet}\")");

         if (node.FirstChildIndex != -1)
         {
            writer.UpIndent();
            nodes.WriteDebugString(rawText, ref writer, node.FirstChildIndex);
            writer.DownIndent();
         }

         if (node.NextSiblingIndex != -1)
         {
            nodes.WriteDebugString(rawText, ref writer, node.NextSiblingIndex);
         }
      }
   }
}