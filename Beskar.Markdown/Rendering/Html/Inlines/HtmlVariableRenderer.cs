using System;
using System.Collections.Generic;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlVariableRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Variable;

   private static readonly ParserOptions _variableEnabledParserOptions = new(
      [.. ParserOptions.Default.BlockParsers],
      [.. ParserOptions.Default.InlineParsers])
   {
      EnableVariables = true
   };

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText,
      ref TextWriterIndentSlim writer,
      in MarkdownNode current,
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var nameSpan = current.TextSpan.Slice(rawText);
      if (nameSpan.IsEmpty)
      {
         return;
      }

      var name = nameSpan.ToString();
      var value = ResolveValue(context, name);

      if (string.IsNullOrEmpty(value))
      {
         return;
      }

      var isBlock = current.VariableIsBlock == 1;

      switch (current.VariableFormat)
      {
         case VariableFormat.Html:
            writer.Write(value);
            if (isBlock && options.AddBlockNewLines)
            {
               writer.WriteLine();
            }
            break;

         case VariableFormat.Markdown:
            RenderMarkdown(context, value, ref writer, options, isBlock);
            break;

         case VariableFormat.Text:
         default:
            if (isBlock)
            {
               writer.Write("<p>");
               writer.WriteHtmlDecodedAndEncoded(value.AsSpan(), encodeApostrophe: false);
               if (options.AddBlockNewLines)
               {
                  writer.WriteLine("</p>");
               }
               else
               {
                  writer.Write("</p>");
               }
            }
            else
            {
               writer.WriteHtmlDecodedAndEncoded(value.AsSpan(), encodeApostrophe: false);
            }
            break;
      }
   }

   private static string? ResolveValue<TData>(MarkdownContext<TData> context, string name)
   {
      if (context.Variables.TryGetValue(name, out var val))
      {
         return val;
      }

      if (context.VariableResolver != null)
      {
         var resolved = context.VariableResolver(name);
         if (resolved != null)
         {
            return resolved;
         }
      }

      if (context.Data is IDictionary<string, string> dict && dict.TryGetValue(name, out var dictVal))
      {
         return dictVal;
      }

      if (context.Data is IReadOnlyDictionary<string, string> roDict && roDict.TryGetValue(name, out var roVal))
      {
         return roVal;
      }

      if (context.Data is IDictionary<string, object> objDict && objDict.TryGetValue(name, out var objVal))
      {
         return objVal?.ToString();
      }

      if (context.Data is IReadOnlyDictionary<string, object> roObjDict && roObjDict.TryGetValue(name, out var roObjVal))
      {
         return roObjVal?.ToString();
      }

      return null;
   }

   private static void RenderMarkdown<TData>(
      MarkdownContext<TData> context,
      string value,
      ref TextWriterIndentSlim writer,
      RenderOptions options,
      bool isBlock)
   {
      var parserOptions = options.EnableVariables ? _variableEnabledParserOptions : ParserOptions.Default;
      var span = value.AsSpan();

      using var subParser = new MarkdownParser<TData>(
         span, new MarkdownNode[Math.Clamp(value.Length / 32, 16, 24)]);
      var subContext = subParser.Parse(parserOptions, context.Data);

      if (context.Variables.Count > 0)
      {
         foreach (var kvp in context.Variables)
         {
            subContext.Variables[kvp.Key] = kvp.Value;
         }
      }
      subContext.VariableResolver = context.VariableResolver;

      var subNodes = subParser.WrittenNodes;
      if (!isBlock && subNodes.Length > 1 && subNodes[0].Type == NodeType.Document)
      {
         var firstChildIdx = subNodes[0].FirstChildIndex;
         if (firstChildIdx != -1 && subNodes[firstChildIdx].NextSiblingIndex == -1 && subNodes[firstChildIdx].Type == NodeType.Paragraph)
         {
            subNodes[firstChildIdx].RenderChildren(subContext, span, subNodes, ref writer, options);
            return;
         }
      }

      var subRenderer = new MarkdownRenderer(span);
      subRenderer.Render(subContext, in subNodes, options, ref writer);
   }
}
