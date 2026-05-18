using Beskar.Markdown.Parsing.Models;

namespace Beskar.Markdown;

public record MarkdownResult<TData>(string Html, MarkdownContext<TData> Context);