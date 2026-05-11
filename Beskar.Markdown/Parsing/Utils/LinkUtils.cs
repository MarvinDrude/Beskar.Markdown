using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Utils;

public static class LinkUtils
{
    public static ReadOnlySpan<char> NormalizeLabel(ReadOnlySpan<char> span, Span<char> destination)
    {
        if (span.IsEmpty) 
            return ReadOnlySpan<char>.Empty;

        var trimmed = span.Trim();
        if (trimmed.IsEmpty) 
            return ReadOnlySpan<char>.Empty;

        var len = 0;
        var lastWasSpace = false;

        foreach (var c in trimmed)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!lastWasSpace)
                {
                    if (len < destination.Length)
                        destination[len++] = ' ';
                    lastWasSpace = true;
                }
            }
            else
            {
                if (len < destination.Length)
                    destination[len++] = char.ToLowerInvariant(c);
                lastWasSpace = false;
            }
        }

        return destination[..len];
    }

    public static string NormalizeLabel(ReadOnlySpan<char> span)
    {
        if (span.IsEmpty) 
            return string.Empty;
        
        var owner = span.Length < 512
            ? new SpanOwner<char>(stackalloc char[span.Length])
            : new SpanOwner<char>(span.Length);
        
        var buffer = owner.Span;
        var normalized = NormalizeLabel(span, buffer);
        
        return new string(normalized);
    }

    public static bool IsAsciiPunctuation(char c)
    {
        return c is >= '!' and <= '/' 
            or >= ':' and <= '@' 
            or >= '[' and <= '`' 
            or >= '{' and <= '~';
    }

    public static bool TryResolveReference<TData>(
        MarkdownContext<TData> context, 
        ReadOnlySpan<char> label, 
        out int nodeIndex)
    {
        if (label.IsEmpty)
        {
            nodeIndex = -1;
            return false;
        }
        
        var owner = label.Length < 512
            ? new SpanOwner<char>(stackalloc char[label.Length])
            : new SpanOwner<char>(label.Length);
        var buffer = owner.Span;
        
        var normalized = NormalizeLabel(label, buffer);
        
        return context.ReferenceDefinitions
            .GetAlternateLookup<ReadOnlySpan<char>>()
            .TryGetValue(normalized, out nodeIndex);
    }
}
