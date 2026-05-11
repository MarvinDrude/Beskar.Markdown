using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Me.Memory.Buffers;
using Beskar.Markdown.Extensions;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class TableParser : IBlockParser
{
    public int Priority => 100;
    public int SupportedTypeValue => (int)NodeType.Table;

    public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
    {
        // tables are started via TryMatchTableDelimiter in MarkdownParser
        return -1;
    }

    public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
        ref BufferWriter<MarkdownNode> writer)
    {
        if (state.IsBlank) return false;

        if (state.LeadingSpaces < 4)
        {
            var c = state.FirstChar;
            if (c is '>' or '#' or '`' or '~') 
                return false;
            
            if (c is '*' or '-' or '_')
            {
                var count = 0;
                var line = state.RawLine[state.FirstNonSpaceIndex..];
                
                for (var i = 0; i < line.Length; i++)
                {
                    if (line[i] == c) count++;
                    else if (line[i] != ' ' && line[i] != '\t')
                    {
                        count = 0;
                        break;
                    }
                }

                if (count >= 3) return false;
            }

            if (c is '*' or '-' or '+')
            {
                var nextIndex = state.FirstNonSpaceIndex + 1;
                if (nextIndex == state.RawLine.Length 
                    || state.RawLine[nextIndex] == ' ' 
                    || state.RawLine[nextIndex] == '\t')
                    return false;
            }

            if (char.IsAsciiDigit(c))
            {
                var i = state.FirstNonSpaceIndex;
                var digitCount = 0;
                
                while (i < state.RawLine.Length 
                    && char.IsAsciiDigit(state.RawLine[i]))
                {
                    i++;
                    digitCount++;
                }

                if (digitCount is > 0 and <= 9
                    && i < state.RawLine.Length
                    && (state.RawLine[i] == '.' || state.RawLine[i] == ')'))
                {
                    i++;
                    
                    if (i == state.RawLine.Length 
                        || state.RawLine[i] == ' ' 
                        || state.RawLine[i] == '\t')
                        return false;
                }
            }
        }

        if (node.LastChildIndex == -1) return false;

        var bodyIndex = node.LastChildIndex;
        ref var body = ref writer.GetReference(bodyIndex);
        
        if (body.Type != NodeType.TableBody) 
            return false; // sanity check

        var rowIndex = writer.WrittenSpan.Length;
        writer.Add(new MarkdownNode
        {
            Type = NodeType.TableRow,
            FirstChildIndex = -1,
            LastChildIndex = -1,
            NextSiblingIndex = -1
        });
        
        LinkNodes(bodyIndex, rowIndex, ref writer);

        var headerIndex = node.FirstChildIndex;
        var headerRowIndex = writer.WrittenSpan[headerIndex].FirstChildIndex;
        var firstCellIndex = writer.WrittenSpan[headerRowIndex].FirstChildIndex;
        
        var columnCount = 0;
        var currentCellIdx = firstCellIndex;
        
        while (currentCellIdx != -1)
        {
            columnCount++;
            currentCellIdx = writer.WrittenSpan[currentCellIdx].NextSiblingIndex;
        }

        long alignments = 0;
        currentCellIdx = firstCellIndex;
        
        for (var i = 0; i < 32 && currentCellIdx != -1; i++)
        {
            long alignment = writer.WrittenSpan[currentCellIdx].TableCellAlignment;
            alignments |= (alignment << (i * 2));
            
            currentCellIdx = writer.WrittenSpan[currentCellIdx].NextSiblingIndex;
        }

        TableUtils.ParseRow(state.RawLine, columnCount, alignments, rowIndex, 
            ref writer, state.GlobalOffset, isHeader: false);
        
        state.ConsumeRest();
        return true;
    }

    private static void LinkNodes(int parentIndex, int childIndex, ref BufferWriter<MarkdownNode> writer)
    {
        ref var parent = ref writer.GetReference(parentIndex);
        
        if (parent.FirstChildIndex == -1)
        {
            parent.FirstChildIndex = childIndex;
            parent.LastChildIndex = childIndex;
            
            return;
        }
        
        ref var lastChild = ref writer.GetReference(parent.LastChildIndex);
        
        lastChild.NextSiblingIndex = childIndex;
        parent.LastChildIndex = childIndex;
    }
}
