using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Beskar.Markdown.Builders;
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

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static string ToHtml(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null)
   {
      return ToContextualHtml<object>(markdown, parserOptions, renderOptions);
   }

   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   public static string ToHtml(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      MarkdownOptions options)
   {
      return ToHtml(markdown, options.ParserOptions, options.RenderOptions);
   }

   public static string ToContextualHtml<TContext>(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null,
      TContext? data = default)
   {
      parserOptions ??= _defaultParserOptions;
      renderOptions ??= _defaultRenderOptions;
       
      using var parser = new MarkdownParser<TContext>(
         markdown, stackalloc MarkdownNode[GetInitialNodeBufferLength(markdown.Length)]);
      var context = parser.Parse(parserOptions, data);
      
      var renderer = new MarkdownRenderer(markdown);
      var writer = new TextWriterIndentSlim(
         stackalloc char[512], stackalloc char[64]);
      
      try
      {
         renderer.Render(context, parser.WrittenNodes, renderOptions, ref writer);

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
   public static MarkdownResult<object> Parse(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      MarkdownOptions? options = null)
   {
      return Parse<object>(markdown, options);
   }

   public static MarkdownResult<TContext> Parse<TContext>(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      MarkdownOptions? options = null,
      TContext? data = default)
   {
      var parserOptions = options?.ParserOptions ?? _defaultParserOptions;
      var renderOptions = options?.RenderOptions ?? _defaultRenderOptions;
       
      using var parser = new MarkdownParser<TContext>(
         markdown, stackalloc MarkdownNode[GetInitialNodeBufferLength(markdown.Length)]);
      var context = parser.Parse(parserOptions, data);
      
      var renderer = new MarkdownRenderer(markdown);
      var writer = new TextWriterIndentSlim(
         stackalloc char[512], stackalloc char[64]);
      
      try
      {
         renderer.Render(context, parser.WrittenNodes, renderOptions, ref writer);

         var html = renderOptions.SanitizerFunc is not null 
            ? renderOptions.SanitizerFunc(writer.WrittenSpan) 
            : writer.ToString();
            
         return new MarkdownResult<TContext>(html, context);
      }
      finally
      {
         writer.Dispose();
      }
   }
   
   [MethodImpl(MethodImplOptions.AggressiveInlining)]
   private static int GetInitialNodeBufferLength(int markdownLength)
   {
      var estimatedNodeCount = markdownLength / 32;
      return Math.Clamp(estimatedNodeCount, 16, 24);
   }
}
