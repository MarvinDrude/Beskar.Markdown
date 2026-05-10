using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Beskar.Markdown.Builders;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;
using Me.Memory.Buffers;

namespace Beskar.Markdown;

public static class BeMarkdown
{
   public const int BuiltInNodeTypeValueOffset = (int)NodeType.Autolink + 1;
   
   private static readonly ParserOptions _defaultParserOptions = ParserOptions.Default;
   private static readonly RenderOptions _defaultRenderOptions = RenderOptions.HtmlDefault;
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static string ToHtml(
      [StringSyntax("Markdown")] string markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null)
   {
      return ToHtml(markdown.AsSpan(), parserOptions, renderOptions);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static string ToHtml(
      [StringSyntax("Markdown")] string markdown, 
      MarkdownOptions options)
   {
      return ToHtml(markdown.AsSpan(), options);
   }

   public static string ToHtml(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null)
   {
      parserOptions ??= _defaultParserOptions;
      renderOptions ??= _defaultRenderOptions;
       
      using var parser = new MarkdownParser(markdown, stackalloc MarkdownNode[GetInitialNodeBufferLength(markdown.Length)]);
      parser.Parse(parserOptions);
      
      var renderer = new MarkdownRenderer(markdown);
      var writer = new TextWriterIndentSlim(
         stackalloc char[512], stackalloc char[64]);
      
      try
      {
         renderer.Render(parser.WrittenNodes, renderOptions, ref writer);

         return renderOptions.SanitizerFunc is not null 
            ? renderOptions.SanitizerFunc(writer.WrittenSpan) 
            : writer.ToString();
      }
      finally
      {
         writer.Dispose();
      }
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static string ToHtml(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      MarkdownOptions options)
   {
      return ToHtml(markdown, options.ParserOptions, options.RenderOptions);
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static int GetInitialNodeBufferLength(int markdownLength)
   {
      var estimatedNodeCount = markdownLength / 32;
      return Math.Clamp(estimatedNodeCount, 16, 24);
   }
}
