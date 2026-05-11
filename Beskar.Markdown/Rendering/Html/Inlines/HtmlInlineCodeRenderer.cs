using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlInlineCodeRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.InlineCode;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.Write("<code>");
      var span = current.TextSpan.Slice(rawText);
      
      if (current.InlineCodeIsInsideTable == 1)
      {
          var i = 0;
          while (i < span.Length)
          {
              var backslashIndex = span[i..].IndexOf('\\');
              if (backslashIndex == -1 || i + backslashIndex + 1 >= span.Length)
              {
                  writer.WriteHtmlEncoded(span[i..]);
                  break;
              }
              
              var absoluteBackslash = i + backslashIndex;
              if (span[absoluteBackslash + 1] == '|')
              {
                  writer.WriteHtmlEncoded(span[i..absoluteBackslash]);
                  writer.Write("|");
                  i = absoluteBackslash + 2;
              }
              else
              {
                  writer.WriteHtmlEncoded(span[i..(absoluteBackslash + 1)]);
                  i = absoluteBackslash + 1;
              }
          }
      }
      else
      {
          writer.WriteHtmlEncoded(span);
      }
      
      writer.Write("</code>");
   }
}